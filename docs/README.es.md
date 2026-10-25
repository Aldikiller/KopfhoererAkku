# BleAirDock

BleAirDock es una aplicación gratuita para Windows que muestra la batería de hasta dos auriculares Bluetooth en la bandeja del sistema o en un widget de escritorio. Cambia entre tus auriculares con un clic y configura avisos de batería baja para recordar cuándo cargarlos. Personaliza el aspecto y los controles, sin anuncios, sin registro y con soporte para cinco idiomas.

Novedades de 1.8.0: menú contextual compacto con selección de audio, visibilidad del widget y más acciones. Controles reúne asignaciones de audio, atajos, volumen e inicio automático. La rueda opcional cambia el volumen del auricular asignado un 2 % por paso sobre su icono o entrada del widget, sin cambiar la salida. El widget puede ocultarse en pantalla completa en su monitor y reaparecer después; la ocultación manual se mantiene. Las dos opciones están desactivadas inicialmente.

Novedades de 1.8.0: reinicio desde el menú contextual; Acerca de BleAirDock y búsqueda de actualizaciones en Ajustes. Se combinan las fuentes de conexión Bluetooth clásicas y modernas de Windows. El widget y la bandeja usan el mismo estado. Se conserva el intervalo configurado.

![BleAirDock](../assets/logo.svg)

BleAirDock se llamaba Kopfhörer-Akku. La versión 1.8.0 incorpora el nuevo nombre y logotipo y conserva las funciones de 1.6.5. La actualización con el instalador conserva ajustes y asignaciones. Las direcciones de GitHub y actualizaciones siguen siendo válidas.

**Versión actual: 1.8.0**

## Novedades de la versión 1.8.0

- Widgets estándar y pequeño: dos iconos pulsables, tamaño, disposición, fuentes, colores y transparencia ajustables. La vista pequeña muestra el nombre y el estado al pasar el ratón; ajuste opcional a los bordes. Puedes ocultar y mostrar el widget desde el menú; se conserva un icono en la bandeja.

- Avisos por auricular con umbral y color propios. Aviso luminoso: 1–90 %, periodo de 1–10 segundos, intensidad de 10–100 % o luz fija. Prueba de 30 segundos en el icono real. Pausa de avisos durante 30, 60 o 120 minutos, sin consultas Bluetooth adicionales.

- Tres atajos personalizables: alternar, activar auriculares 1 y activar auriculares 2. Combina Ctrl, Alt, Mayús y Win con A–Z. Inicialmente desactivados; se informa de combinaciones no disponibles.

- Memoria de volumen opcional por auricular: fija el primer nivel; después, la aplicación recuerda el volumen al dejar un dispositivo y lo restaura al seleccionar su salida. No cambia el silencio ni el micrófono.

- Importa PNG/ICO en Nombres y pantalla → Elegir icono. Hay cinco ejemplos SVG y PNG en docs/symbols. SVG es la fuente editable; se importa PNG/ICO. El widget pequeño puede mostrar solo la imagen. Más contraste en la bandeja; el widget conserva los colores originales.

- Los estados distinguen desconexión, conexión desconocida, ausencia de nivel de batería de Windows y error de consulta. Los valores guardados no se muestran como una medición actual confirmada.

## Selección de audio también desde el widget

Haz clic en el icono, el nombre o el porcentaje de los auriculares en el widget para seleccionar su salida de audio. Una marca verde indica la salida activa. Arrastrar mueve el widget sin cambiar el audio; el doble clic abre los ajustes. También funciona con la posición bloqueada. Comparte las asignaciones y la opción de selección rápida con los iconos de la bandeja.

## Cambiar la salida de audio con un clic

Haz clic en el icono de batería para cambiar la salida de audio de Windows a esos auriculares. Un círculo verde con una marca blanca indica la salida activa. Los dispositivos con una coincidencia única se asignan automáticamente; si no, elige la salida una vez en la selección rápida de audio. Se conservan las asignaciones manuales.

![Audio output](Audio-Direktwahl-1.6.0.png)

La batería se consulta cada 30 segundos por defecto, ajustable entre 30 y 300 segundos. Sin consultas rápidas adicionales ni avisos rutinarios de progreso. Las actualizaciones se comprueban manualmente o, si lo activas, al iniciar. · Español

[Descargar](https://github.com/Aldikiller/KopfhoererAkku/releases/latest) · [Código fuente](https://github.com/Aldikiller/KopfhoererAkku) · [Preguntas y experiencias](https://github.com/Aldikiller/KopfhoererAkku/discussions)

Consulta la batería de hasta dos auriculares Bluetooth junto al reloj de Windows, en un widget de escritorio o en ambos. Una pequeña aplicación de Aldikiller.

## Cambiar el idioma

Abre Einstellungen → Allgemein → Sprache / Language, elige Español y pulsa Speichern. La interfaz cambia al instante y recuerda la selección. El idioma predeterminado es alemán.

## Instalación

Descarga el instalador o descomprime completamente el ZIP portátil. Enciende los auriculares, conéctalos a Windows y selecciona hasta dos dispositivos en la aplicación. Si hay entradas duplicadas, elige la que muestra el nivel de batería.

## Personalízalo

Cambia nombres, colores, símbolos, fuentes y la forma de la batería. La vista previa muestra los cambios antes de guardarlos.

## Widget y ajustes

Haz doble clic en un icono para abrir los ajustes y clic derecho para abrir el menú. Bajo Pantalla y widget puedes elegir la bandeja, el widget o ambos. Ajusta tamaño, posición, transparencia, fuentes y disposición. Para un fondo transparente, pon la opacidad del fondo y el grosor del borde a cero.

## Avisos a tiempo

Avisos de batería baja, inicio opcional con Windows y búsqueda voluntaria de actualizaciones. No se instalan actualizaciones automáticamente.

## Requisitos y límites

Windows 10 desde la compilación 19041 o Windows 11, x64, con .NET Framework 4.8. Solo se muestran los datos que proporcionan Windows y el controlador. Probado con JBL Live 770NC y JLab Rewind 2. Otros modelos aún no están confirmados. Los datos pueden llegar con retraso. Los archivos aún no tienen firma digital. Windows puede mostrar una advertencia.

## Ayuda

Si falta un dispositivo, comprueba la conexión de Windows, vuelve a seleccionarlo y espera el siguiente intervalo. Busca los iconos ocultos bajo ^ junto al reloj. Los avisos requieren una conexión confirmada; No molestar puede ocultarlos.

## Privacidad y licencia

Sin publicidad ni telemetría. Los ajustes permanecen en tu ordenador. La búsqueda opcional de actualizaciones está desactivada inicialmente. Al usarla, GitHub recibe tu IP y la versión de la aplicación, pero no nombres de dispositivos ni niveles de batería. Código abierto bajo licencia MIT, sin garantía en la medida permitida por la ley.

## Desinstalación

Elimina la versión instalada desde Configuración de Windows → Aplicaciones. Los ajustes permanecen en %LOCALAPPDATA%\KopfhoererAkku. Para la versión portátil, desactiva el inicio automático, cierra la aplicación y elimina su carpeta.

Comparte el modelo de auriculares, las versiones de Windows y de la aplicación y los pasos para reproducir el problema. No publiques direcciones Bluetooth ni datos privados.

MIT: [Código fuente](https://github.com/Aldikiller/KopfhoererAkku/blob/main/LICENSE)
