using System.Globalization;
using GBlock.App.Base;

namespace GBlock.App.ViewModels;

/// <summary>Uma linha do calendario semanal (segunda a domingo).</summary>
public class DiaLimiteViewModel : ObservableObject
{
    private bool _liberado;
    private int _horas;
    private int _minutos;

    public DiaLimiteViewModel(DayOfWeek dia, int totalMinutos)
    {
        Dia = dia;
        _liberado = totalMinutos > 0;
        _horas = totalMinutos / 60;
        _minutos = totalMinutos % 60;
    }

    public DayOfWeek Dia { get; }

    public string NomeDia => CultureInfo.GetCultureInfo("pt-BR")
        .DateTimeFormat.GetDayName(Dia) is { } nome
        ? char.ToUpper(nome[0], CultureInfo.CurrentCulture) + nome[1..]
        : Dia.ToString();

    public bool Liberado
    {
        get => _liberado;
        set
        {
            if (!Definir(ref _liberado, value))
                return;

            if (value && TotalMinutos == 0)
                Horas = 1;

            OnPropertyChanged(nameof(TotalMinutos));
            OnPropertyChanged(nameof(Resumo));
        }
    }

    public int Horas
    {
        get => _horas;
        set
        {
            if (Definir(ref _horas, Math.Clamp(value, 0, 23)))
                Recalcular();
        }
    }

    public int Minutos
    {
        get => _minutos;
        set
        {
            if (Definir(ref _minutos, Math.Clamp(value, 0, 59)))
                Recalcular();
        }
    }

    public int TotalMinutos => Liberado ? Horas * 60 + Minutos : 0;

    public string Resumo => TotalMinutos == 0
        ? "sem tempo liberado"
        : $"{Horas}h {Minutos:00}min por dia";

    public void Aplicar(int totalMinutos)
    {
        Liberado = totalMinutos > 0;
        Horas = totalMinutos / 60;
        Minutos = totalMinutos % 60;
    }

    private void Recalcular()
    {
        OnPropertyChanged(nameof(TotalMinutos));
        OnPropertyChanged(nameof(Resumo));
    }
}
