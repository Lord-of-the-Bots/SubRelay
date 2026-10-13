using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Windows;

namespace SubRelay.Services;

public static class Localization
{
    private static readonly CultureInfo SystemCulture = ReadWindowsLanguage();
    private static string _language = Resolve(SystemCulture.Name);
    public static string CurrentLanguage => _language;
    public static event Action? Changed;

    // The same semantic keys are available to code and WPF DynamicResource bindings.
    // Entries are Russian, English, Brazilian Portuguese and Spanish, in that order.
    private static readonly Dictionary<string, string[]> Strings = new(StringComparer.Ordinal)
    {
        ["CustomChannelMapping"] = ["Свои каналы", "Custom channels", "Canais próprios", "Canales propios"],
        ["CustomChannelMappingHelp"] = ["Выберите выходные каналы и отдельный диапазон частот для каждого. В неотмеченные каналы Sub Relay звук не отправляет.", "Choose output channels and set a separate frequency range for each. Unchecked channels receive no audio from Sub Relay.", "Escolha os canais de saída e uma faixa de frequências para cada um. Canais desmarcados não recebem áudio do Sub Relay.", "Elige los canales de salida y un rango de frecuencias para cada uno. Los canales desmarcados no reciben audio de Sub Relay."],
        ["OpenChannelMapper"] = ["Настроить каналы", "Open mapper", "Configurar canais", "Configurar canales"],
        ["ChannelMapperTitle"] = ["Настройка каналов", "Channel mapper", "Configuração de canais", "Configuración de canales"],
        ["ChannelMapperHelp"] = ["Выберите каналы, подключённые к сабвуферам. У каждого канала свой фильтр частот. Уровень баса общий для звуковой карты.", "Select the channels connected to your subwoofers. Each channel has its own frequency filter. Bass level applies to the whole sound card.", "Selecione os canais conectados aos subwoofers. Cada canal tem seu próprio filtro de frequências. O nível de graves vale para toda a placa de som.", "Selecciona los canales conectados a tus subwoofers. Cada canal tiene su propio filtro de frecuencias. El nivel de graves se aplica a toda la tarjeta de sonido."],
        ["ChannelMapperSilence"] = ["Каналы не выбраны: Sub Relay не будет отправлять звук на эту карту.", "No channels selected: this sound card will receive no audio from Sub Relay.", "Nenhum canal selecionado: esta placa não receberá áudio do Sub Relay.", "No hay canales seleccionados: esta tarjeta no recibirá audio de Sub Relay."],
        ["ChannelMapperDeviceChanged"] = ["Устройство или схема каналов изменились. Закройте и заново откройте настройку каналов перед сохранением.", "The device or its channel layout changed. Close and reopen the mapper before saving.", "O dispositivo ou a disposição dos canais mudou. Feche e abra novamente a configuração antes de salvar.", "El dispositivo o la distribución de canales ha cambiado. Cierra y vuelve a abrir la configuración antes de guardar."],
        ["ChannelMapperUnavailable"] = ["Подключите звуковую карту, чтобы настроить её каналы.", "Connect a sound card to configure its channels.", "Conecte uma placa de som para configurar os canais.", "Conecta una tarjeta de sonido para configurar sus canales."],
        ["SaveChannelMapping"] = ["Сохранить", "Save", "Salvar", "Guardar"],
        ["CancelChannelMapping"] = ["Отмена", "Cancel", "Cancelar", "Cancelar"],
        ["ChannelNameFormat"] = ["{0}. {1}", "{0}. {1}", "{0}. {1}", "{0}. {1}"],
        ["ChannelUnknown"] = ["Канал {0}", "Channel {0}", "Canal {0}", "Canal {0}"],
        ["ChannelFrontLeft"] = ["Передний левый", "Front left", "Frontal esquerdo", "Frontal izquierdo"],
        ["ChannelFrontRight"] = ["Передний правый", "Front right", "Frontal direito", "Frontal derecho"],
        ["ChannelFrontCenter"] = ["Передний центральный", "Front center", "Frontal central", "Frontal central"],
        ["ChannelLfe"] = ["LFE / сабвуфер", "LFE / subwoofer", "LFE / subwoofer", "LFE / subwoofer"],
        ["ChannelBackLeft"] = ["Задний левый", "Back left", "Traseiro esquerdo", "Trasero izquierdo"],
        ["ChannelBackRight"] = ["Задний правый", "Back right", "Traseiro direito", "Trasero derecho"],
        ["ChannelFrontLeftCenter"] = ["Передний левее центра", "Front left of center", "Frontal à esquerda do centro", "Frontal a la izquierda del centro"],
        ["ChannelFrontRightCenter"] = ["Передний правее центра", "Front right of center", "Frontal à direita do centro", "Frontal a la derecha del centro"],
        ["ChannelBackCenter"] = ["Задний центральный", "Back center", "Traseiro central", "Trasero central"],
        ["ChannelSideLeft"] = ["Боковой левый", "Side left", "Lateral esquerdo", "Lateral izquierdo"],
        ["ChannelSideRight"] = ["Боковой правый", "Side right", "Lateral direito", "Lateral derecho"],
        ["ChannelTopCenter"] = ["Верхний центральный", "Top center", "Superior central", "Superior central"],
        ["ChannelTopFrontLeft"] = ["Верхний передний левый", "Top front left", "Superior frontal esquerdo", "Superior frontal izquierdo"],
        ["ChannelTopFrontCenter"] = ["Верхний передний центральный", "Top front center", "Superior frontal central", "Superior frontal central"],
        ["ChannelTopFrontRight"] = ["Верхний передний правый", "Top front right", "Superior frontal direito", "Superior frontal derecho"],
        ["ChannelTopBackLeft"] = ["Верхний задний левый", "Top back left", "Superior traseiro esquerdo", "Superior trasero izquierdo"],
        ["ChannelTopBackCenter"] = ["Верхний задний центральный", "Top back center", "Superior traseiro central", "Superior trasero central"],
        ["ChannelTopBackRight"] = ["Верхний задний правый", "Top back right", "Superior traseiro direito", "Superior trasero derecho"],
        ["SubwooferRunningMapped"] = ["Индивидуальные каналы включены", "Custom channel mapping active", "Mapeamento de canais ativo", "Asignación de canales activa"],
        ["ChannelMappingSilent"] = ["Выходные каналы не выбраны", "No output channels selected", "Nenhum canal de saída selecionado", "No hay canales de salida seleccionados"],
        ["Introduction"] = ["Музыка и кино — с глубоким басом.", "Deeper bass for music and movies.", "Mais graves para músicas e filmes.", "Más graves para música y películas."],
        ["Settings"] = ["Настройки", "Settings", "Configurações", "Ajustes"],
        ["CloseToTray"] = ["Закрывать в трей", "Close to system tray", "Minimizar para a bandeja ao fechar", "Minimizar a la bandeja al cerrar"],
        ["StartWithWindows"] = ["Запускать с Windows", "Start with Windows", "Iniciar com o Windows", "Iniciar con Windows"],
        ["Language"] = ["Язык", "Language", "Idioma", "Idioma"],
        ["SystemLanguage"] = ["Как в системе", "System default", "Padrão do sistema", "Idioma del sistema"],
        ["RefreshDevices"] = ["Обновить устройства", "Refresh devices", "Atualizar dispositivos", "Actualizar dispositivos"],
        ["Exit"] = ["Выйти", "Exit", "Sair", "Salir"],
        ["Pause"] = ["Пауза", "Pause", "Pausar", "Pausar"],
        ["Resume"] = ["Продолжить", "Resume", "Retomar", "Reanudar"],
        ["PauseHelp"] = ["Остановить передачу баса на все выбранные выходы. Основные колонки продолжат работать.", "Stop bass output to all selected devices. Your main speakers keep playing.", "Interromper os graves em todas as saídas selecionadas. As caixas principais continuam tocando.", "Detener los graves en todas las salidas seleccionadas. Los altavoces principales siguen sonando."],
        ["ManuallyPaused"] = ["Sub Relay на ручной паузе. Нажмите «Продолжить» в Sub Relay.", "Sub Relay is manually paused. Choose Resume in Sub Relay.", "O Sub Relay foi pausado manualmente. Escolha Retomar no Sub Relay.", "Sub Relay está en pausa manual. Elige Reanudar en Sub Relay."],
        ["AddSubwoofer"] = ["＋ Добавить сабвуфер", "＋ Add subwoofer", "＋ Adicionar subwoofer", "＋ Añadir subwoofer"],
        ["SelectSubwooferDevice"] = ["Выберите выход сабвуфера {0}", "Select output for subwoofer {0}", "Selecione a saída do subwoofer {0}", "Selecciona la salida del subwoofer {0}"],
        ["SubwooferDevice"] = ["Выход сабвуфера", "Subwoofer output", "Saída do subwoofer", "Salida del subwoofer"],
        ["NoneSelected"] = ["Не выбрано", "Not selected", "Não selecionado", "Sin seleccionar"],
        ["SoundCard"] = ["Звуковая карта", "Sound card", "Placa de som", "Tarjeta de sonido"],
        ["UnavailableDevice"] = ["{0} (недоступна)", "{0} (unavailable)", "{0} (indisponível)", "{0} (no disponible)"],
        ["BassLevel"] = ["Уровень баса", "Bass level", "Nível de graves", "Nivel de graves"],
        ["BassPercent"] = ["Уровень баса, процентов", "Bass level, percent", "Nível de graves, porcentagem", "Nivel de graves, porcentaje"],
        ["LowCut"] = ["Не ниже, Гц", "Min, Hz", "Mín., Hz", "Mín., Hz"],
        ["HighCut"] = ["Не выше, Гц", "Max, Hz", "Máx., Hz", "Máx., Hz"],
        ["FrequencyHelp"] = ["Начните с верхней границы 80 Гц и подберите её под колонки и сабвуфер. Нижняя граница 0 отключает обрезку снизу. Допустимо 0–200 Гц; нижняя граница должна быть меньше верхней.", "Start with an 80 Hz upper limit and adjust to your speakers and subwoofer. A lower limit of 0 disables the low cut. Range: 0–200 Hz; the lower limit must be below the upper limit.", "Comece com o limite superior de 80 Hz e ajuste às caixas e ao subwoofer. O limite inferior 0 desativa o corte de graves. Faixa: 0–200 Hz; o limite inferior deve ser menor que o superior.", "Empieza con un límite superior de 80 Hz y ajústalo a tus altavoces y subwoofer. El límite inferior 0 desactiva el corte de graves. Rango: 0–200 Hz; el límite inferior debe ser menor que el superior."],
        ["SubwooferUnavailable"] = ["Сабвуфер недоступен", "Subwoofer unavailable", "Subwoofer indisponível", "Subwoofer no disponible"],
        ["SameDeviceHelp"] = ["Эту карту сейчас использует Windows. Выберите другой основной выход Windows — сабвуфер включится сам.", "Windows is currently using this card as its default output. Select another default output in Windows and the subwoofer will resume automatically.", "O Windows está usando esta placa como saída padrão. Selecione outra saída padrão no Windows e o subwoofer será reativado automaticamente.", "Windows está usando esta tarjeta como salida predeterminada. Elige otra salida predeterminada en Windows y el subwoofer se reactivará automáticamente."],
        ["FrequencyValidation"] = ["Введите числа от 0 до 200. Первое должно быть меньше второго. Esc — отменить изменения.", "Enter numbers from 0 to 200. The first must be lower than the second. Esc cancels changes.", "Digite números de 0 a 200. O primeiro deve ser menor que o segundo. Esc cancela as alterações.", "Introduce números de 0 a 200. El primero debe ser menor que el segundo. Esc cancela los cambios."],
        ["OpenApp"] = ["Открыть Sub Relay", "Open Sub Relay", "Abrir Sub Relay", "Abrir Sub Relay"],
        ["TrayTitle"] = ["Sub Relay — бас на сабвуферы", "Sub Relay — bass to your subwoofers", "Sub Relay — graves nos seus subwoofers", "Sub Relay — graves para tus subwoofers"],
        ["TrayHint"] = ["Работа продолжается в трее. Двойной щелчок по значку откроет окно; «Выйти» завершит приложение.", "Sub Relay is still running in the system tray. Double-click its icon to open the window, or choose Exit to close the app.", "O Sub Relay continua na bandeja do sistema. Clique duas vezes no ícone para abrir a janela ou escolha Sair para encerrar.", "Sub Relay sigue funcionando en la bandeja del sistema. Haz doble clic en el icono para abrir la ventana o elige Salir para cerrar la aplicación."],
        ["UiError"] = ["Ошибка Sub Relay: {0}", "Sub Relay error: {0}", "Erro do Sub Relay: {0}", "Error de Sub Relay: {0}"],
        ["StartupError"] = ["Sub Relay — ошибка запуска", "Sub Relay — startup error", "Sub Relay — erro ao iniciar", "Sub Relay — error al iniciar"],
        ["SaveSettingsError"] = ["Не удалось сохранить настройки: {0}", "Could not save settings: {0}", "Não foi possível salvar as configurações: {0}", "No se pudieron guardar los ajustes: {0}"],
        ["LoadSettingsWarning"] = ["Не удалось прочитать настройки. Используются значения по умолчанию.", "Could not read settings. Using default settings.", "Não foi possível ler as configurações. Serão usados os valores padrão.", "No se pudieron leer los ajustes. Se usarán los valores predeterminados."],
        ["MissingAppIcon"] = ["Не найдена иконка приложения.", "Application icon not found.", "Ícone do aplicativo não encontrado.", "No se encontró el icono de la aplicación."],
        ["InvalidAppPath"] = ["Некорректный путь приложения.", "Invalid application path.", "Caminho do aplicativo inválido.", "La ruta de la aplicación no es válida."],
        ["StartupSettingsUnavailable"] = ["Не удалось открыть настройки автозапуска Windows.", "Could not open Windows startup settings.", "Não foi possível abrir as configurações de inicialização do Windows.", "No se pudieron abrir los ajustes de inicio de Windows."],
        ["AppPathUnknown"] = ["Не определён путь приложения.", "Could not determine the application path.", "Não foi possível determinar o caminho do aplicativo.", "No se pudo determinar la ruta de la aplicación."],
        ["DefaultDeviceBlocked"] = ["Это устройство сейчас выбрано основным выходом Windows. Дублирование отключено, чтобы не создавать звуковую петлю. Выберите другой основной выход в Windows — сабвуфер включится автоматически.", "This device is the default Windows output. Duplication is disabled to prevent an audio loop. Select another default Windows output and the subwoofer will resume automatically.", "Este dispositivo é a saída padrão do Windows. A duplicação está desativada para evitar um ciclo de áudio. Selecione outra saída padrão no Windows e o subwoofer será reativado automaticamente.", "Este dispositivo es la salida predeterminada de Windows. La duplicación está desactivada para evitar un bucle de audio. Elige otra salida predeterminada en Windows y el subwoofer se reactivará automáticamente."],
        ["DuplicateDeviceBlocked"] = ["Это устройство уже используется другим сабвуфером. Выберите отдельную звуковую карту или удалите дубликат.", "Another subwoofer already uses this device. Select a different sound card or remove the duplicate.", "Outro subwoofer já usa este dispositivo. Selecione outra placa de som ou remova a duplicata.", "Otro subwoofer ya usa este dispositivo. Selecciona otra tarjeta de sonido o elimina el duplicado."],
        ["FindingDefaultDevice"] = ["Поиск основного устройства Windows…", "Finding the default Windows output…", "Buscando a saída padrão do Windows…", "Buscando la salida predeterminada de Windows…"],
        ["AudioOpenError"] = ["Не удалось открыть аудиоустройства. Повторная попытка автоматически. {0}", "Could not open audio devices. Will retry automatically. {0}", "Não foi possível abrir os dispositivos de áudio. Uma nova tentativa será feita automaticamente. {0}", "No se pudieron abrir los dispositivos de audio. Se reintentará automáticamente. {0}"],
        ["AudioServiceUnavailable"] = ["Аудиослужба Windows недоступна", "Windows audio service is unavailable", "O serviço de áudio do Windows está indisponível", "El servicio de audio de Windows no está disponible"],
        ["DefaultDeviceMissing"] = ["Основное устройство вывода не найдено", "No default output device found", "Nenhuma saída de áudio padrão encontrada", "No se encontró una salida de audio predeterminada"],
        ["CaptureStopped"] = ["Захват системного звука остановлен.", "System audio capture stopped.", "A captura do áudio do sistema parou.", "Se detuvo la captura del audio del sistema."],
        ["SubwooferDisabled"] = ["Выключен", "Disabled", "Desativado", "Desactivado"],
        ["SelectDevice"] = ["Выберите звуковую карту сабвуфера", "Select a subwoofer sound card", "Selecione a placa de som do subwoofer", "Selecciona la tarjeta de sonido del subwoofer"],
        ["DeviceDisconnected"] = ["Устройство отключено. Подключите его — работа возобновится автоматически.", "Device disconnected. Reconnect it to resume automatically.", "Dispositivo desconectado. Reconecte-o para retomar automaticamente.", "Dispositivo desconectado. Vuelve a conectarlo para reanudar automáticamente."],
        ["WaitingDefaultDevice"] = ["Ожидание основного устройства Windows", "Waiting for the default Windows output", "Aguardando a saída padrão do Windows", "Esperando la salida predeterminada de Windows"],
        ["InvalidFrequencyBand"] = ["Введите частоты от 0 до 200 Гц. Первая должна быть меньше второй.", "Enter frequencies from 0 to 200 Hz. The first must be lower than the second.", "Digite frequências de 0 a 200 Hz. A primeira deve ser menor que a segunda.", "Introduce frecuencias de 0 a 200 Hz. La primera debe ser menor que la segunda."],
        ["PlaybackStopped"] = ["Воспроизведение остановлено.", "Playback stopped.", "A reprodução parou.", "Se detuvo la reproducción."],
        ["CaptureError"] = ["Не удалось захватить системный звук. {0}", "Could not capture system audio. {0}", "Não foi possível capturar o áudio do sistema. {0}", "No se pudo capturar el audio del sistema. {0}"],
        ["CaptureUnavailable"] = ["Захват звука временно недоступен.", "Audio capture is temporarily unavailable.", "A captura de áudio está temporariamente indisponível.", "La captura de audio no está disponible temporalmente."],
        ["RetryAutomatically"] = ["{0} Повторная попытка автоматически.", "{0} Will retry automatically.", "{0} Uma nova tentativa será feita automaticamente.", "{0} Se reintentará automáticamente."],
        ["SubwooferRunning"] = ["Работает · {0}–{1} Гц", "Running · {0}–{1} Hz", "Ativo · {0}–{1} Hz", "Activo · {0}–{1} Hz"],
        ["OutputOpenError"] = ["Не удалось включить устройство. {0}", "Could not start the device. {0}", "Não foi possível iniciar o dispositivo. {0}", "No se pudo iniciar el dispositivo. {0}"],
        ["ErrorCode"] = ["Код 0x{0}.", "Code 0x{0}.", "Código 0x{0}.", "Código 0x{0}."],
        ["AudioProcessingError"] = ["Ошибка обработки системного звука. {0}", "System audio processing error. {0}", "Erro ao processar o áudio do sistema. {0}", "Error al procesar el audio del sistema. {0}"],
        ["OutputStopped"] = ["Устройство вывода остановлено.", "Output device stopped.", "O dispositivo de saída parou.", "Se detuvo el dispositivo de salida."],
        ["ConnectingDevice"] = ["Подключение…", "Connecting…", "Conectando…", "Conectando…"],
        ["OtherInstanceOwnsAudio"] = ["Звук уже обрабатывает другая копия Sub Relay. Закройте её, чтобы использовать эту копию.", "Another copy of Sub Relay is already processing audio. Close it to use this copy.", "Outra instância do Sub Relay já está processando o áudio. Feche-a para usar esta instância.", "Otra instancia de Sub Relay ya está procesando el audio. Ciérrala para usar esta instancia."],
        ["SourceHeading"] = ["Звук с основного выхода Windows", "Audio from the default Windows output", "Áudio da saída padrão do Windows", "Audio de la salida predeterminada de Windows"],
        ["LevelNote"] = ["Уровень баса зависит от громкости захватываемого звука.", "Bass level depends on the volume of the captured audio.", "O nível de graves depende do volume do áudio capturado.", "El nivel de graves depende del volumen del audio capturado."],
        ["LevelHelp"] = ["Сначала настройте громкость в плеере, затем уровень баса здесь. Общая громкость устройства Windows может не влиять на захват. Начните с небольшой громкости усилителя сабвуфера.", "Adjust your player volume first, then the bass level here. The Windows device volume may not affect capture. Start with a low volume on the subwoofer amplifier.", "Ajuste primeiro o volume do player, depois o nível de graves aqui. O volume do dispositivo Windows pode não afetar a captura. Comece com o volume baixo no amplificador do subwoofer.", "Ajusta primero el volumen del reproductor y después el nivel de graves aquí. El volumen del dispositivo Windows puede no afectar a la captura. Empieza con un volumen bajo en el amplificador del subwoofer."],
        ["SetupHelp"] = ["Подключите активный сабвуфер к отдельному аудиовыходу.", "Connect a powered subwoofer to a separate audio output.", "Conecte um subwoofer ativo a uma saída de áudio separada.", "Conecta un subwoofer activo a una salida de audio independiente."],
    };

    public static string Text(string key, params object[] args)
    {
        if (!Strings.TryGetValue(key, out var values)) return key;
        string language = _language;
        int index = language switch { "ru" => 0, "pt-BR" => 2, "es" => 3, _ => 1 };
        return args.Length == 0 ? values[index] : string.Format(CultureInfo.GetCultureInfo(language), values[index], args);
    }

    public static bool IsText(string key, string value) =>
        Strings.TryGetValue(key, out var translations) && Array.Exists(translations, text => text == value);

    public static void Apply(string? language, ResourceDictionary? resources = null)
    {
        _language = Resolve(string.IsNullOrWhiteSpace(language) || language!.Equals("system", StringComparison.OrdinalIgnoreCase)
            ? SystemCulture.Name : language);
        ResourceDictionary? target = resources ?? Application.Current?.Resources;
        if (target is not null)
            foreach (string key in Strings.Keys) target[key] = Text(key);
        Changed?.Invoke();
    }

    private static string Resolve(string language)
    {
        if (language.StartsWith("ru", StringComparison.OrdinalIgnoreCase)) return "ru";
        if (language.StartsWith("pt", StringComparison.OrdinalIgnoreCase)) return "pt-BR";
        if (language.StartsWith("es", StringComparison.OrdinalIgnoreCase)) return "es";
        return "en";
    }

    private static CultureInfo ReadWindowsLanguage()
    {
        // Use the Windows user's display language without changing thread culture.
        try { return CultureInfo.GetCultureInfo(GetUserDefaultUILanguage()); }
        catch (CultureNotFoundException) { return CultureInfo.CurrentUICulture; }
    }

    [DllImport("kernel32.dll", ExactSpelling = true)]
    private static extern ushort GetUserDefaultUILanguage();
}
