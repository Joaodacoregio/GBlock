# GBlock

Controle de tempo de tela por processo no Windows. Você escolhe um processo (da lista dos que
estão rodando ou digitando o nome do executável), define quanto tempo ele pode ficar aberto em
cada dia da semana, e o GBlock cuida do resto: avisa quando o tempo está acabando, fecha o
processo quando acaba e impede que ele seja reaberto até o dia seguinte.

## Arquitetura

Mesmo desenho do CoachPro — camadas separadas por projeto, dependência sempre apontando para o
domínio:

```
src/
  GBlock.Domain          modelos, enums e interfaces (sem dependências)
  GBlock.Service         regras de negócio: AvaliadorRegra + MonitorService
  GBlock.Infrastructure  persistência JSON, acesso a processos, auto-start no registro
  GBlock.App             WPF (MVVM) + ícone de bandeja
```

`GBlock.slnx` organiza os projetos em `01-Application`, `02-Domain`, `03-Service`,
`04-Infrastructure`.

## Como rodar

```bash
dotnet run --project "src/GBlock.App/GBlock.App.csproj"
```

Para gerar o executável (self-contained, arquivo único — não exige .NET instalado na máquina):

```bash
dotnet publish "src/GBlock.App/GBlock.App.csproj" -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:EnableCompressionInSingleFile=true -o publish
```

Sai um `publish/GBlock.exe` de ~72 MB. Os `.pdb` ao lado são só para depuração e podem ser
descartados na distribuição. O argumento `--tray` inicia direto na bandeja, sem abrir a janela —
é o que a opção "Iniciar junto com o Windows" grava no registro.

## Como funciona

- **Monitor** (`MonitorService`): verifica a cada 2 segundos quais processos monitorados estão
  abertos e acumula o tempo. Grava o consumo em disco a cada ~30 segundos e ao sair.
- **Avisos**: aparece um card flutuante no canto inferior direito (sempre no topo) quando falta o
  tempo configurado na regra, e de novo aos 5 e ao 1 minuto restante. Também sai um balão na
  bandeja.
- **Encerramento**: ao zerar o saldo do dia, o processo é finalizado (`Kill` com a árvore de
  processos).
- **Bloqueio de abertura**: como a verificação é contínua, abrir o jogo com o saldo zerado faz o
  processo ser fechado no tique seguinte, com o aviso "Bloqueado por hoje".
- **Dados**: `%AppData%\GBlock\regras.json` e `%AppData%\GBlock\uso.json` (histórico de 90 dias).

## Duas decisões que vale conhecer

**Não é um Serviço do Windows.** Um serviço roda na sessão 0 e não consegue abrir janelas nem
colocar ícone na bandeja — precisaria de um segundo processo só para a UI, com IPC entre os dois.
Como o que você precisa (contar tempo, avisar, fechar) só faz sentido enquanto o usuário está
logado, o GBlock roda como app de bandeja com auto-start pela chave `Run` do usuário (o checkbox
"Iniciar junto com o Windows" na tela principal liga/desliga isso). Se depois você quiser o
serviço de verdade para impedir que o usuário simplesmente feche o GBlock pelo Gerenciador de
Tarefas, o `MonitorService` já está isolado do WPF e pode ser hospedado num Worker Service sem
alteração — só entra a camada de IPC com o app de bandeja.

**Sem elevação.** O `app.manifest` usa `asInvoker`. Jogos que rodam como administrador (alguns
anticheat) não vão poder ser encerrados assim; nesse caso troque para `requireAdministrator` no
manifesto.

## Limitações conhecidas

- O bloqueio é reativo (fecha até 2s depois de abrir), não impede o `CreateProcess`. Bloqueio real
  na abertura exigiria driver ou política do Windows.
- Quem tem acesso ao `%AppData%` pode editar o `uso.json` e recuperar tempo.
- O tempo conta enquanto o processo existe, não enquanto a janela está em foco.
