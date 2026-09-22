using GBlock.Domain.Enums;
using GBlock.Domain.Interfaces;
using GBlock.Domain.Models;
using GBlock.Service.Regras;

namespace GBlock.Service.Monitoramento;

/// <summary>
/// Nucleo do GBlock: a cada intervalo contabiliza o tempo dos processos monitorados,
/// avisa quando o tempo esta acabando, encerra ao estourar o limite e impede a
/// reabertura enquanto o dia continuar sem saldo.
/// </summary>
public class MonitorService : IMonitorService, IDisposable
{
    private static readonly TimeSpan Intervalo = TimeSpan.FromSeconds(2);
    private const int TicksEntreGravacoes = 15; // ~30s

    private readonly IRegraRepository _regras;
    private readonly IUsoRepository _usos;
    private readonly IProcessoProvider _processos;

    private readonly object _trava = new();
    private readonly Dictionary<Guid, ControleAviso> _avisos = new();

    private Timer? _timer;
    private UsoDiario _usoDoDia = new() { Data = DateOnly.FromDateTime(DateTime.Today) };
    private IReadOnlyList<EstadoRegra> _ultimoEstado = [];
    private int _ticksDesdeGravacao;
    private bool _emVerificacao;

    public MonitorService(IRegraRepository regras, IUsoRepository usos, IProcessoProvider processos)
    {
        _regras = regras;
        _usos = usos;
        _processos = processos;
    }

    public event EventHandler<IReadOnlyList<EstadoRegra>>? EstadoAtualizado;
    public event EventHandler<EstadoRegra>? AvisoDeEncerramento;
    public event EventHandler<EstadoRegra>? ProcessoEncerrado;
    public event EventHandler<EstadoRegra>? AberturaBloqueada;

    public void Iniciar()
    {
        lock (_trava)
        {
            if (_timer is not null)
                return;

            _usoDoDia = _usos.ObterDia(DateOnly.FromDateTime(DateTime.Today));
            _timer = new Timer(_ => Verificar(), null, TimeSpan.Zero, Intervalo);
        }
    }

    public void Parar()
    {
        lock (_trava)
        {
            _timer?.Dispose();
            _timer = null;
            _usos.Salvar(_usoDoDia);
        }
    }

    public IReadOnlyList<EstadoRegra> EstadoAtual() => _ultimoEstado;

    public void Verificar()
    {
        // Um tick lento (matar processo, por exemplo) nao pode empilhar verificacoes.
        lock (_trava)
        {
            if (_emVerificacao)
                return;

            _emVerificacao = true;
        }

        try
        {
            ExecutarVerificacao();
        }
        finally
        {
            lock (_trava)
                _emVerificacao = false;
        }
    }

    private void ExecutarVerificacao()
    {
        var hoje = DateOnly.FromDateTime(DateTime.Today);
        VirarDiaSeNecessario(hoje);

        var estados = new List<EstadoRegra>();
        var houveMudanca = false;

        foreach (var regra in _regras.Listar())
        {
            if (!regra.Ativa)
            {
                // Regra pausada continua visivel na interface, mas nao consome nem bloqueia.
                estados.Add(AvaliadorRegra.Avaliar(regra, hoje.DayOfWeek, _usoDoDia.Segundos(regra.Id), false));
                continue;
            }

            var emExecucao = _processos.ContarInstancias(regra.NomeProcesso) > 0;
            var controle = Controle(regra.Id);

            if (emExecucao && PodeContabilizar(regra, hoje))
            {
                _usoDoDia.Acumular(regra.Id, (int)Intervalo.TotalSeconds);
                houveMudanca = true;
            }

            var estado = AvaliadorRegra.Avaliar(regra, hoje.DayOfWeek, _usoDoDia.Segundos(regra.Id), emExecucao);
            estados.Add(estado);

            Reagir(estado, controle);
        }

        _ultimoEstado = estados;
        EstadoAtualizado?.Invoke(this, estados);

        if (houveMudanca && ++_ticksDesdeGravacao >= TicksEntreGravacoes)
        {
            _ticksDesdeGravacao = 0;
            _usos.Salvar(_usoDoDia);
        }
    }

    /// <summary>Nao consome saldo de quem ja estourou o limite — evita "divida" no dia seguinte.</summary>
    private bool PodeContabilizar(RegraProcesso regra, DateOnly hoje)
    {
        var limite = TimeSpan.FromMinutes(regra.LimiteMinutos(hoje.DayOfWeek));
        return limite > TimeSpan.Zero && TimeSpan.FromSeconds(_usoDoDia.Segundos(regra.Id)) < limite;
    }

    private void Reagir(EstadoRegra estado, ControleAviso controle)
    {
        switch (estado.Status)
        {
            case StatusRegra.Liberado:
                controle.MarcoAvisado = int.MaxValue;
                controle.JaEncerrou = false;
                break;

            case StatusRegra.Aviso:
                controle.JaEncerrou = false;
                AvisarSeNecessario(estado, controle);
                break;

            case StatusRegra.Bloqueado:
            case StatusRegra.DiaBloqueado:
                Bloquear(estado, controle);
                break;
        }
    }

    private void AvisarSeNecessario(EstadoRegra estado, ControleAviso controle)
    {
        if (!estado.EmExecucao)
            return;

        var restanteMinutos = (int)Math.Ceiling(estado.Restante.TotalMinutes);

        foreach (var marco in AvaliadorRegra.MarcosDeAviso(estado.Regra))
        {
            if (restanteMinutos <= marco && marco < controle.MarcoAvisado)
            {
                controle.MarcoAvisado = marco;
                AvisoDeEncerramento?.Invoke(this, estado);
                return;
            }
        }
    }

    private void Bloquear(EstadoRegra estado, ControleAviso controle)
    {
        if (!estado.EmExecucao)
        {
            // Saiu sozinho: a proxima abertura conta como tentativa bloqueada.
            controle.JaEncerrou = false;
            return;
        }

        var encerrados = _processos.Encerrar(estado.Regra.NomeProcesso);
        if (encerrados == 0)
            return;

        if (controle.JaEncerrou)
            AberturaBloqueada?.Invoke(this, estado);
        else
            ProcessoEncerrado?.Invoke(this, estado);

        controle.JaEncerrou = true;
        controle.MarcoAvisado = 0;
    }

    private void VirarDiaSeNecessario(DateOnly hoje)
    {
        if (_usoDoDia.Data == hoje)
            return;

        _usos.Salvar(_usoDoDia);
        _usoDoDia = _usos.ObterDia(hoje);
        _avisos.Clear();
    }

    private ControleAviso Controle(Guid regraId)
    {
        if (!_avisos.TryGetValue(regraId, out var controle))
        {
            controle = new ControleAviso();
            _avisos[regraId] = controle;
        }

        return controle;
    }

    public void Dispose() => Parar();

    /// <summary>Evita repetir o mesmo aviso/encerramento a cada tick.</summary>
    private sealed class ControleAviso
    {
        public int MarcoAvisado { get; set; } = int.MaxValue;
        public bool JaEncerrou { get; set; }
    }
}
