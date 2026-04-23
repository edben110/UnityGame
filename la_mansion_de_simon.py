# ═══════════════════════════════════════════════════════════════════
#  LA MANSIÓN DE SIMÓN
#  Una historia de secretos, silencios y supervivencia
# ═══════════════════════════════════════════════════════════════════

import os, time, sys, textwrap, random

# ── Configuración visual ──────────────────────────────────────────

W = 66
VELOCIDAD = 0.018
VELOCIDAD_LENTA = 0.028

def limpiar():
    os.system('cls' if os.name == 'nt' else 'clear')

def _wrap(texto, indent=2):
    lineas = textwrap.wrap(texto, W - indent)
    return [" " * indent + l for l in lineas]

def beep():
    """Sonido del sistema si está disponible."""
    try:
        sys.stdout.write('\a')
        sys.stdout.flush()
    except Exception:
        pass

def escribir(texto, vel=VELOCIDAD):
    for linea in _wrap(texto):
        for c in linea:
            sys.stdout.write(c); sys.stdout.flush(); time.sleep(vel)
        print()
    print()

def escribir_lento(texto):
    escribir(texto, VELOCIDAD_LENTA)

def escribir_miedo(texto):
    """Texto que aparece con ritmo irregular, como si algo interfiriera."""
    for linea in _wrap(texto):
        for c in linea:
            sys.stdout.write(c); sys.stdout.flush()
            if random.random() < 0.08:
                time.sleep(random.uniform(0.15, 0.4))
            else:
                time.sleep(0.02)
        print()
    print()

def narrar(texto):
    """Texto narrativo con sangría y ritmo."""
    for linea in _wrap(texto, 4):
        print(linea)
    print()

def dialogo(nombre, texto, pausa_final=True):
    prefijo = f"  {nombre.upper()}: "
    sys.stdout.write(prefijo)
    envuelto = textwrap.wrap(texto, W - len(prefijo))
    for i, linea in enumerate(envuelto):
        if i > 0:
            sys.stdout.write(" " * len(prefijo))
        for c in linea:
            sys.stdout.write(c); sys.stdout.flush(); time.sleep(0.015)
        print()
    if pausa_final:
        print()

def dialogo_roto(nombre, texto, pausa_final=True):
    """Diálogo entrecortado, como si costara hablar."""
    prefijo = f"  {nombre.upper()}: "
    sys.stdout.write(prefijo)
    for c in texto:
        sys.stdout.write(c); sys.stdout.flush()
        if c == '.':
            time.sleep(random.uniform(0.5, 1.2))
        elif c == ' ':
            time.sleep(random.uniform(0.08, 0.3))
        else:
            time.sleep(random.uniform(0.03, 0.08))
    print()
    if pausa_final:
        print()

def acotacion(texto):
    """Texto entre paréntesis, más sutil."""
    for linea in _wrap(texto, 6):
        print(f"\033[2m{linea}\033[0m")
    print()

def separador():
    print(f"\n  {'─' * (W - 4)}\n")

def pausa(msg=None):
    if msg:
        print(f"  {msg}")
    input(f"\n  {'▸ Presiona ENTER para continuar...':^{W}}\n")

def pausa_tension(segundos=2):
    """Pausa silenciosa que genera tensión."""
    time.sleep(segundos)

def glitch(texto="", intensidad=1):
    """Efecto de interferencia visual. La pantalla 'falla'."""
    chars_glitch = "█▓▒░╔╗╚╝║═╬┼┤├┬┴"
    for _ in range(intensidad):
        linea = "  " + "".join(random.choice(chars_glitch) for _ in range(W - 4))
        sys.stdout.write(f"\r{linea}")
        sys.stdout.flush()
        time.sleep(0.06)
        sys.stdout.write(f"\r{' ' * W}")
        sys.stdout.flush()
        time.sleep(0.03)
    if texto:
        sys.stdout.write(f"\r")
        for c in f"  {texto}":
            sys.stdout.write(c); sys.stdout.flush(); time.sleep(0.04)
        print()
    else:
        sys.stdout.write(f"\r{' ' * W}\r")
        sys.stdout.flush()

def pantalla_negra(segundos=2):
    """Pantalla completamente vacía por un tiempo."""
    limpiar()
    time.sleep(segundos)

def texto_sangre(texto):
    """Texto en rojo (si la terminal lo soporta)."""
    print(f"\033[31m\033[1m  {texto}\033[0m\n")

def titulo(texto, sub=""):
    limpiar()
    print()
    print(f"  {'═' * (W - 4)}")
    print(f"{texto:^{W}}")
    if sub:
        print(f"\033[2m{sub:^{W}}\033[0m")
    print(f"  {'═' * (W - 4)}")
    print()

def titulo_horror(texto, sub=""):
    """Título que aparece con efecto de glitch."""
    limpiar()
    print()
    for _ in range(3):
        glitch(intensidad=2)
        time.sleep(0.1)
    print(f"  {'═' * (W - 4)}")
    beep()
    for c in texto.center(W):
        sys.stdout.write(c); sys.stdout.flush(); time.sleep(0.04)
    print()
    if sub:
        print(f"\033[2m{sub:^{W}}\033[0m")
    print(f"  {'═' * (W - 4)}")
    print()

def elegir(opciones):
    print()
    for i, op in enumerate(opciones, 1):
        print(f"    [{i}] {op}")
    print()
    while True:
        try:
            r = input("    ▸ ").strip()
            e = int(r)
            if 1 <= e <= len(opciones):
                print()
                return e
        except (ValueError, EOFError):
            pass
        print("    Opción inválida. Intenta de nuevo.")

# ── Estado del juego ──────────────────────────────────────────────

class Personaje:
    def __init__(self, nombre, rol, edad, ocupacion, secreto, objeto, ubicacion_obj):
        self.nombre = nombre
        self.rol = rol
        self.edad = edad
        self.ocupacion = ocupacion
        self.secreto = secreto
        self.objeto = objeto
        self.ubicacion_obj = ubicacion_obj
        self.aislamiento = 0
        self.presente = True
        self.interactuado_cap = False
        self.objeto_encontrado = False
        self.confianza = 0  # -100 a 100

    def aislar(self, n):
        if self.presente:
            self.aislamiento = min(100, self.aislamiento + n)

    def conectar(self, n):
        if self.presente:
            self.aislamiento = max(0, self.aislamiento - n)
            self.confianza = min(100, self.confianza + n)
            self.interactuado_cap = True

    def barra(self):
        lleno = self.aislamiento // 10
        vacio = 10 - lleno
        if self.aislamiento >= 75:
            tag = "⚠ CRÍTICO"
        elif self.aislamiento >= 50:
            tag = "▲ alto"
        elif self.aislamiento >= 25:
            tag = "~ medio"
        else:
            tag = "✓ estable"
        return f"{'█' * lleno}{'░' * vacio} {self.aislamiento:3d}  {tag}"


class Estado:
    def __init__(self):
        self.personajes = {}
        self.pistas = set()
        self.decisiones = []
        self.capitulo = 0
        self.simon_encontrado = False
        self.codigo_encontrado = False
        self._crear_personajes()

    def _crear_personajes(self):
        datos = [
            ("Ben", "El Dinero", 38, "Corredor de bolsa en quiebra",
             "Desvió fondos de las ventas de Simón durante años",
             "Libro de cuentas comprometedor", "Habitación de Simón"),
            ("Lisa", "La Evidencia", 31, "Periodista suspendida",
             "Tiene copias de pruebas de un crimen que Simón presenció",
             "Carpeta con fotografías y documentos", "Galería"),
            ("Robert", "La Carta", 54, "Abogado notarial semi-retirado",
             "Es hermano secreto de Simón por parte de padre",
             "Carta manuscrita del padre común", "Lobby"),
            ("Ana", "Las Joyas", 45, "Galerista y tasadora de arte",
             "Usó joyas familiares de Simón como garantía sin permiso",
             "Estuche de cuero con joyas familiares", "Estudio"),
            ("Lucas", "El Relicario", 27, "Estudiante, ex-ayudante de Simón",
             "Tomó un relicario de plata que pertenecía a Simón",
             "Relicario de plata con inscripción", "Sala de vigilancia"),
        ]
        for d in datos:
            p = Personaje(*d)
            self.personajes[d[0]] = p

    def presentes(self):
        return [n for n, p in self.personajes.items() if p.presente]

    def p(self, nombre):
        return self.personajes[nombre]

    def tiene(self, pista):
        return pista in self.pistas

    def guardar_pista(self, pista):
        self.pistas.add(pista)

    def decidir(self, desc):
        self.decisiones.append(desc)

    def fin_capitulo(self):
        """Aplica mecánica de aislamiento al final de cada capítulo."""
        for p in self.personajes.values():
            if p.presente and not p.interactuado_cap:
                p.aislar(25)
        todos = all(p.presente for p in self.personajes.values())
        if todos:
            for p in self.personajes.values():
                p.conectar(5)
        for p in self.personajes.values():
            p.interactuado_cap = False

    def verificar_abandonos(self):
        candidatos = [(n, p.aislamiento) for n, p in self.personajes.items()
                      if p.presente and p.aislamiento >= 75]
        if not candidatos:
            return None
        candidatos.sort(key=lambda x: -x[1])
        nombre = candidatos[0][0]
        self.personajes[nombre].presente = False
        return nombre

    def mostrar_grupo(self):
        separador()
        print("  ESTADO DEL GRUPO")
        print()
        for nombre, p in self.personajes.items():
            if p.presente:
                print(f"    {nombre:8s}  {p.barra()}")
            else:
                print(f"    {nombre:8s}  ──────────  ✝ MUERTO")
        separador()


G = Estado()

# ══════════════════════════════════════════════════════════════
#  PRÓLOGO
# ══════════════════════════════════════════════════════════════

def prologo():
    limpiar()
    print("\n" * 5)
    time.sleep(1)
    # Efecto de aparición lenta del título
    titulo_texto = "L A   M A N S I Ó N   D E   S I M Ó N"
    for c in titulo_texto.center(W):
        sys.stdout.write(c); sys.stdout.flush(); time.sleep(0.06)
    print()
    time.sleep(0.5)
    print(f"\033[2m{'Una historia de secretos, silencios y supervivencia':^{W}}\033[0m")
    time.sleep(2)
    print("\n" * 2)
    pausa()

    # Pantalla negra antes de la premisa
    pantalla_negra(1.5)

    titulo("P R E M I S A")
    print(f"\033[2m{'Europa, 1943. La guerra devora el continente.':^{W}}\033[0m")
    print(f"\033[2m{'Pero aquí, lejos del frente, hay otros tipos de guerra.':^{W}}\033[0m")
    print()
    time.sleep(1)
    escribir_lento("Son las tres de la tarde.")
    time.sleep(0.5)
    narrar("Una mansión antigua, de paredes que huelen a barniz viejo "
           "y a secretos acumulados durante décadas, recibe a cinco "
           "desconocidos que, sin embargo, no lo son del todo entre sí.")
    narrar("Los une un nombre: Simón. Un pintor. Un hombre que, según "
           "les comunicaron hace pocos días, ha muerto.")
    pausa()

    narrar("Pero cada uno llegó con algo más que condolencias. Cada uno "
           "llegó con una razón propia, silenciosa, que no piensa "
           "compartir con nadie. Y todos, sin excepción, buscan algo "
           "dentro de esa mansión.")
    narrar("Lo que ninguno sabe es que no están solos. En algún rincón "
           "de la casa, alguien más aguarda. Alguien cuya razón para "
           "estar ahí es mucho más oscura que todas las demás juntas.")
    print()
    escribir_lento("El jugador observa. El jugador explora.")
    escribir_lento("El jugador decide quién sobrevive.")
    pausa()

    # Presentación de personajes
    titulo("L O S   P E R S O N A J E S")
    for nombre, p in G.personajes.items():
        print(f"  ◆ {nombre.upper()} — {p.rol}")
        print(f"    {p.edad} años. {p.ocupacion}.")
        acotacion(p.secreto + ".")
    narrar("Y en algún lugar de la mansión, oculto, retenido, esperando: "
           "Simón. El pintor. El hombre que todos creen muerto.")
    pausa()

    titulo("M E C Á N I C A   D E   J U E G O")
    narrar("No es el asesino lo que mata a los personajes directamente. "
           "Es la soledad. El asesino solo termina lo que el aislamiento "
           "empezó.")
    narrar("Cada personaje tiene un nivel de aislamiento. Si lo ignoras, "
           "crece. Si hablas con ellos, baja. Si el aislamiento de alguien "
           "supera el umbral crítico al final de un capítulo, esa persona "
           "se separará del grupo. Y quien se separa del grupo en esta "
           "mansión no sobrevive.")
    narrar("Tus decisiones determinan quién vive, quién muere, qué pistas "
           "descubres, y cómo termina esta historia.")
    narrar("Explora cada escenario. Habla con todos. Encuentra a Simón "
           "antes de que sea demasiado tarde.")
    pausa()

# ══════════════════════════════════════════════════════════════
#  CAPÍTULO 1 — LA LLEGADA (3:00 PM)
# ══════════════════════════════════════════════════════════════

def capitulo_1():
    G.capitulo = 1
    titulo("C A P Í T U L O   1", "La Llegada — 3:00 PM")
    time.sleep(0.5)

    narrar("El primero en llegar fue Robert. Lo hizo con diez minutos de "
           "adelanto, como hacía siempre, como si la puntualidad fuera una "
           "forma de demostrar que tenía el control de algo.")
    narrar("El lobby olía a polvo y a rosas secas. Techos altos con molduras "
           "de yeso amarillento, una escalera central de madera oscura, "
           "sillones de cuero rojo envejecido, una chimenea apagada. "
           "Un gramófono de latón descansaba en la esquina, con un disco "
           "puesto que nadie recordaba haber colocado. En la radio de "
           "válvulas junto a la ventana, solo estática.")
    pausa()

    narrar("Ana llegó a las tres en punto. Entró con la seguridad de quien "
           "conoce los espacios de arte. Saludó a Robert con una inclinación "
           "de cabeza, sin sonreír. Ninguno preguntó qué hacía el otro allí.")
    narrar("Ben llegó tres minutos después, con la corbata aflojada y una "
           "expresión de alguien que lleva semanas sin dormir bien. "
           "Lanzó un 'buenas tardes' demasiado animado que no convenció a nadie.")
    narrar("Lucas fue el cuarto. Llegó con un maletín de cuero gastado y "
           "un sombrero que se quitó al entrar. Cuando vio a los otros tres, "
           "se detuvo un momento, sorprendido de no ser el único.")
    narrar("Lisa llegó la última, a las tres y cuarto. Entró mirando cada "
           "detalle del espacio con ojos profesionales. Eligió el sillón "
           "más cercano a la puerta de entrada.")
    narrar("Durante unos minutos nadie habló. Luego fue Ben quien rompió "
           "el silencio con un comentario sobre el clima. El grupo empezó, "
           "lentamente, a intercambiar palabras que no decían nada real.")
    pausa()

    # ── Exploración del Lobby ──
    explorar_lobby()

    # ── Interacciones ──
    titulo("E L   L O B B Y", "Interacciones — Capítulo 1")
    narrar("Los cinco están dispersos por el lobby. Puedes hablar con "
           "quien quieras. Cada conversación importa.")
    interacciones_c1()

    # ── Decisión ──
    titulo("D E C I S I Ó N", "Capítulo 1")
    narrar("Llevan una hora en el lobby. La tensión es palpable. "
           "Nadie ha dicho por qué está realmente aquí. "
           "El silencio empieza a pesar.")
    e = elegir([
        "Proponer explorar la mansión juntos, como grupo",
        "Sugerir que cada uno explore por su cuenta",
        "Intentar que el grupo hable abiertamente sobre Simón",
    ])
    if e == 1:
        G.decidir("grupo_unido_c1")
        narrar("El grupo acepta, aunque con reservas. Hay algo reconfortante "
               "en moverse juntos por una casa que ninguno conoce del todo. "
               "Los pasos de cinco personas suenan distintos a los de una sola.")
        for n in G.presentes(): G.p(n).conectar(10)
    elif e == 2:
        G.decidir("separados_c1")
        narrar("Cada uno toma una dirección distinta. La mansión los absorbe "
               "en silencio. La distancia entre ellos crece con cada paso. "
               "Desde algún lugar de la casa, alguien observa cómo se separan.")
        for n in G.presentes(): G.p(n).aislar(15)
    else:
        G.decidir("hablar_c1")
        narrar("Las respuestas son vagas, calculadas. Pero en los silencios "
               "entre las palabras hay más información que en las palabras mismas. "
               "Algo se mueve debajo de la superficie de cada frase.")
        for n in G.presentes(): G.p(n).conectar(5)

    _cierre_capitulo()


def explorar_lobby():
    titulo("E L   L O B B Y", "Exploración libre")
    narrar("La luz entra por ventanas emplomadas que filtran el sol de la "
           "tarde en franjas anaranjadas. Hay flores secas en un jarrón "
           "sobre la repisa. Un abrigo cuelga del perchero cerca de la puerta.")
    visto = set()
    while True:
        ops = []
        if "libro" not in visto:
            ops.append("Examinar el libro de visitas sobre la mesita")
        if "abrigo" not in visto:
            ops.append("Revisar el abrigo en el perchero")
        if "foto" not in visto:
            ops.append("Mirar la fotografía sobre la chimenea")
        if "periodico" not in visto:
            ops.append("Leer el periódico doblado en el sillón")
        ops.append("Terminar de explorar")
        e = elegir(ops)
        sel = ops[e - 1]

        if "libro de visitas" in sel:
            visto.add("libro")
            narrar("Sobre la mesita de entrada hay un libro donde los visitantes "
                   "de la mansión firmaban su llegada. Las últimas tres entradas "
                   "son de los últimos seis meses.")
            narrar("Una de ellas está tachada con tinta roja. El nombre debajo "
                   "de la tachadura es ilegible, pero la fecha es de hace "
                   "exactamente tres semanas.")
            G.guardar_pista("entrada_tachada")

        elif "abrigo" in sel:
            visto.add("abrigo")
            narrar("Un abrigo de hombre de talla grande. En el bolsillo interior "
                   "hay una nota manuscrita, sin firma:")
            dialogo("NOTA", "No confíes en nadie que llegue antes que tú.")
            acotacion("La letra es firme, decidida. No es la letra de alguien "
                      "que escribe con miedo. Es la de alguien que advierte.")
            G.guardar_pista("nota_abrigo")

        elif "fotografía" in sel:
            visto.add("foto")
            narrar("Una fotografía en blanco y negro de Simón joven, junto a un "
                   "hombre mayor de rasgos severos. Al dorso, escrito a lápiz:")
            dialogo("DORSO", "Padre e hijo. 1987.")
            acotacion("Robert, al otro lado de la sala, desvía la mirada "
                      "cuando notas que examinas la fotografía.")
            G.guardar_pista("foto_padre_hijo")
            G.p("Robert").aislar(10)

        elif "periódico" in sel:
            visto.add("periodico")
            narrar("Un periódico local de hace tres días. La esquina de la "
                   "página de sucesos está doblada. El titular visible dice:")
            dialogo("TITULAR", "Incendio en almacén del puerto, investigación reabierta.")
            acotacion("Lisa, desde su sillón, mira el periódico con una "
                      "expresión que intenta ser indiferente y no lo consigue.")
            G.guardar_pista("incendio_periodico")
            G.p("Lisa").conectar(10)

        else:
            break


def interacciones_c1():
    hablados = set()
    while True:
        vivos = [n for n in G.presentes() if n not in hablados]
        if not vivos:
            narrar("Ya hablaste con todos los presentes.")
            break
        ops = [f"Hablar con {n} — {G.p(n).rol}" for n in vivos]
        ops.append("No hablar con nadie más")
        e = elegir(ops)
        if e > len(vivos):
            break
        nombre = vivos[e - 1]
        hablados.add(nombre)
        G.p(nombre).conectar(20)
        _dialogo_c1(nombre)


def _dialogo_c1(nombre):
    separador()
    if nombre == "Robert":
        dialogo("ROBERT", "Vine porque me lo pidieron. Un abogado, no sé "
                "quién lo contrató, me envió una carta diciéndome que había "
                "asuntos pendientes relacionados con la herencia de Simón "
                "que requerían mi presencia.")
        acotacion("Hace una pausa. Su postura es rígida, ensayada.")
        dialogo("ROBERT", "No lo conocía bien. Nos cruzamos en algunos "
                "círculos. Nada más.", False)
        acotacion("Lo dice con demasiada calma. La clase de calma que se ensaya "
                  "frente al espejo antes de salir de casa.")

    elif nombre == "Ana":
        dialogo("ANA", "Era mi cliente. Uno de los mejores que he tenido, "
                "honestamente. Cuando me dijeron que había muerto...")
        acotacion("Deja la frase en el aire.")
        dialogo("ANA", "Supongo que vine por respeto. Y porque alguien tiene "
                "que asegurarse de que su obra quede bien catalogada. "
                "No es una colección menor.", False)
        acotacion("Sonríe brevemente. Sus ojos recorren la sala como "
                  "tasando cada objeto visible.")

    elif nombre == "Ben":
        dialogo("BEN", "Simón y yo éramos socios, de cierta forma. Él pintaba, "
                "yo me encargaba del lado financiero. Muy informal, ya sabe. "
                "Sin contratos de por medio. La gente creativa suele preferirlo así.")
        acotacion("Saca un reloj de bolsillo, lo mira, lo guarda.")
        dialogo("BEN", "La verdad es que me enteré de su muerte y quise "
                "venir a... no sé, despedirme. Algo así.", False)
        acotacion("Hay algo en su voz que no alcanza a esconder. "
                  "Urgencia disfrazada de nostalgia.")

    elif nombre == "Lisa":
        dialogo("LISA", "Lo conocí en una exposición. Hace tres años, creo. "
                "Éramos amigos. Buenos amigos.")
        acotacion("Mira hacia la escalera.")
        dialogo("LISA", "¿Alguien sabe qué pasó exactamente? La noticia fue "
                "muy vaga. Muerte repentina, dicen. Pero eso no significa nada.", False)
        acotacion("Lo dice como quien ya tiene una teoría y busca "
                  "que alguien la confirme sin saberlo.")

    elif nombre == "Lucas":
        dialogo("LUCAS", "Yo trabajé para él. Ayudante de estudio, básicamente. "
                "Limpiaba pinceles, preparaba lienzos, a veces mezclaba pigmentos.")
        acotacion("Una pausa.")
        dialogo("LUCAS", "Era muy exigente. Pero aprendí más con él que en "
                "tres años de facultad. No sé por qué vine. Supongo que quería "
                "ver el lugar una última vez.", False)
        acotacion("Lo dice mirando al suelo. Algo en su postura sugiere "
                  "que hay mucho más detrás de esas palabras.")
    pausa()

# ══════════════════════════════════════════════════════════════
#  CAPÍTULO 2 — LA PRIMERA HORA (4:00 PM)
# ══════════════════════════════════════════════════════════════

def capitulo_2():
    G.capitulo = 2
    titulo("C A P Í T U L O   2", "La Primera Hora — 4:00 PM")
    time.sleep(0.5)

    narrar("La luz había cambiado. El sol de la tarde se había movido y "
           "ahora entraba por el lado opuesto, proyectando sombras más "
           "largas sobre el suelo de madera del lobby.")
    narrar("Fue Ana quien descubrió que la puerta del estudio no estaba "
           "cerrada con llave. La empujó sin pensar demasiado y la encontró "
           "abierta. Nadie protestó cuando el grupo comenzó a explorar.")
    pausa()

    explorar_estudio()
    interacciones_c2()

    titulo("D E C I S I Ó N", "Capítulo 2")
    narrar("Ben está nervioso. Sus ojos vuelven una y otra vez al libro "
           "de contabilidad. Lisa mira la fotografía del corcho con una "
           "expresión que mezcla reconocimiento y miedo.")
    e = elegir([
        "Confrontar a Ben sobre las entradas marcadas con B roja",
        "Preguntarle a Lisa quién cree que es la persona de la cinta negra",
        "Proponer subir al segundo piso juntos",
    ])
    if e == 1:
        G.decidir("confrontar_ben_c2")
        narrar("Ben se pone rígido. Tarda un segundo demasiado largo.")
        dialogo("BEN", "Simón y yo teníamos un acuerdo informal. Esas entradas "
                "son transacciones normales. No sé por qué están marcadas así.")
        acotacion("Nadie le cree. Pero nadie lo dice en voz alta. "
                  "El silencio que sigue es más elocuente que cualquier acusación.")
        G.p("Ben").aislar(15)
        G.guardar_pista("ben_confrontado")
    elif e == 2:
        G.decidir("preguntar_lisa_c2")
        acotacion("Lisa baja la voz y mira hacia la puerta.")
        dialogo("LISA", "Simón me habló una vez de alguien. Alguien que sabía "
                "cosas que no debía saber sobre cierto asunto. Me dijo que era "
                "peligroso. No me dijo el nombre.")
        acotacion("Hay algo en la forma en que lo dice que sugiere que Lisa "
                  "sabe más de lo que comparte. Pero al menos está hablando.")
        G.p("Lisa").conectar(20)
        G.guardar_pista("lisa_confia")
    else:
        G.decidir("subir_juntos_c2")
        narrar("El grupo sube en silencio. La escalera cruje bajo sus pasos. "
               "El piso superior pertenece a un territorio más íntimo, más "
               "personal. Nadie había querido ir arriba todavía.")
        for n in G.presentes(): G.p(n).conectar(5)

    _cierre_capitulo()


def explorar_estudio():
    titulo("E L   E S T U D I O   D E   S I M Ó N", "Exploración libre")
    narrar("Escritorio de madera maciza cubierto de papeles, libros apilados "
           "sin orden aparente, una lámpara de pantalla verde, un archivador "
           "metálico con tres cajones. Huele a café frío y a papel húmedo.")
    visto = set()
    while True:
        ops = []
        if "agenda" not in visto:
            ops.append("Examinar la agenda de escritorio")
        if "contabilidad" not in visto:
            ops.append("Revisar el libro de contabilidad abierto")
        if "nota" not in visto:
            ops.append("Leer la nota clavada en el tablón del escritorio")
        if "corcho" not in visto:
            ops.append("Estudiar el tablero de corcho con fotografías")
        if "archivador" not in visto:
            ops.append("Intentar abrir el archivador (cajón superior cerrado)")
        ops.append("Terminar de explorar")
        e = elegir(ops)
        sel = ops[e - 1]

        if "agenda" in sel:
            visto.add("agenda")
            narrar("La agenda del año en curso está abierta en el mes actual. "
                   "Varios días tienen entradas crípticas: nombres de personas "
                   "y horas. El último día registrado, tres días atrás, dice:")
            dialogo("AGENDA", "Reunión cancelada. Peligro.")
            acotacion("Tres días. Simón escribió esto tres días antes de que "
                      "todos recibieran la noticia de su muerte.")
            G.guardar_pista("agenda_peligro")

        elif "contabilidad" in sel:
            visto.add("contabilidad")
            narrar("Un libro de registros con columnas de fechas y montos. "
                   "Los últimos registros muestran discrepancias significativas "
                   "en los ingresos de ventas de los últimos cinco años. "
                   "Hay entradas marcadas con una B roja.")
            acotacion("B de Ben. Las cifras no mienten, aunque las personas sí.")
            G.guardar_pista("libro_contabilidad")
            G.p("Ben").aislar(10)

        elif "nota" in sel:
            visto.add("nota")
            narrar("Una nota clavada con un alfiler en el tablón sobre el escritorio:")
            dialogo("NOTA", "Segunda copia hecha. Ellos no saben que "
                    "tengo el duplicado.")
            acotacion("Sin contexto adicional. Pero la palabra 'ellos' "
                      "implica que Simón sabía que alguien lo vigilaba.")
            G.guardar_pista("segunda_copia")

        elif "corcho" in sel:
            visto.add("corcho")
            narrar("Un tablero de corcho con fotografías de personas. Cinco "
                   "de ellas tienen nombres escritos debajo: Ben, Lisa, Robert, "
                   "Ana, Lucas. Una sexta fotografía, en el centro, muestra a "
                   "una persona cuyo rostro ha sido cubierto con cinta negra.")
            acotacion("Simón los conocía a todos. Los tenía identificados. "
                      "Y a alguien más, alguien cuyo rostro decidió ocultar.")
            G.guardar_pista("foto_cinta_negra")

        elif "archivador" in sel:
            visto.add("archivador")
            narrar("El cajón superior está cerrado con llave. A través de la "
                   "ranura se puede ver el borde de una carpeta de cuero marrón.")
            acotacion("Necesitarás una llave para abrirlo. Quizás haya una "
                      "en algún otro lugar de la mansión.")
            G.guardar_pista("archivador_cerrado")
        else:
            break


def interacciones_c2():
    titulo("E L   E S T U D I O", "Interacciones — Capítulo 2")
    hablados = set()
    while True:
        vivos = [n for n in G.presentes() if n not in hablados]
        if not vivos:
            narrar("Ya hablaste con todos.")
            break
        ops = [f"Hablar con {n}" for n in vivos] + ["No hablar con nadie más"]
        e = elegir(ops)
        if e > len(vivos): break
        nombre = vivos[e - 1]
        hablados.add(nombre)
        G.p(nombre).conectar(20)
        _dialogo_c2(nombre)


def _dialogo_c2(nombre):
    separador()
    if nombre == "Robert":
        dialogo("ROBERT", "Hay algo perturbador en este cuarto. El hecho de "
                "que todo esté en orden, ¿no le parece? Si alguien muere de "
                "repente, se esperaría cierto caos. Pero aquí todo está "
                "colocado con cuidado.")
        acotacion("Examina los lomos de los libros sin tocarlos.")
        dialogo("ROBERT", "Como si hubiera tenido tiempo de ordenarlo "
                "antes de irse.", False)

    elif nombre == "Ana":
        dialogo("ANA", "Ese libro de registros. Simón nunca llevó sus propias "
                "cuentas. Era una de las cosas que siempre me dijo, que los "
                "números lo aburrían. Que para eso estaban los que entendían "
                "de negocios.")
        acotacion("Sus ojos se detienen un momento en las letras B rojas.")
        dialogo("ANA", "Interesante que tenga esto aquí.", False)

    elif nombre == "Ben":
        acotacion("Ben está de espaldas cuando te acercas. Cuando se gira, "
                  "hay algo en su expresión que no alcanza a esconder.")
        dialogo("BEN", "Estaba mirando los bocetos. Simón tenía un talento "
                "brutal para el retrato, ¿verdad? Lo que me pregunto es quién "
                "tiene acceso a sus papeles ahora que no está. ¿Un abogado? "
                "¿La familia?")
        acotacion("Hay urgencia en su voz, aunque intenta que suene "
                  "como curiosidad.")

    elif nombre == "Lisa":
        acotacion("Lisa baja la voz.")
        dialogo("LISA", "Esa fotografía en el corcho. La de la cinta negra. "
                "Yo sé quién podría ser. O al menos tengo una idea.")
        acotacion("Mira hacia la puerta para asegurarse de que nadie escucha.")
        dialogo("LISA", "Simón me habló una vez de alguien. Alguien que sabía "
                "cosas que no debía saber sobre cierto asunto. Me dijo que era "
                "peligroso. No me dijo el nombre.", False)

    elif nombre == "Lucas":
        acotacion("Está mirando los bocetos con una intensidad que va más "
                  "allá de la admiración artística.")
        dialogo("LUCAS", "Este boceto es mío. Me lo hizo sin que yo lo supiera. "
                "Un día lo vi aquí colgado y se lo mencioné.")
        dialogo("LUCAS", "Me dijo que los mejores retratos se hacen cuando "
                "el sujeto no sabe que lo están mirando.", False)
    pausa()

# ══════════════════════════════════════════════════════════════
#  CAPÍTULO 3 — LA TARDE SILENCIOSA (5:00 PM)
# ══════════════════════════════════════════════════════════════

def capitulo_3():
    G.capitulo = 3
    titulo("C A P Í T U L O   3", "La Tarde Silenciosa — 5:00 PM")
    time.sleep(0.5)

    narrar("Nadie había querido ir arriba todavía. Como si hubiera un "
           "acuerdo tácito de que el piso superior pertenecía a un "
           "territorio distinto, más personal, más íntimo.")
    narrar("La puerta del pasillo del segundo piso estaba abierta, y al "
           "final del corredor, la habitación principal no tenía llave.")
    pausa()

    explorar_habitacion()
    interacciones_c3()

    titulo("D E C I S I Ó N", "Capítulo 3")
    narrar("La carta inconclusa sigue en el suelo. 'No estoy muerto. "
           "Estoy en...' Si Simón está vivo, está en algún lugar de esta "
           "mansión. Y alguien lo puso ahí.")
    e = elegir([
        "Mostrar la carta al grupo y proponer buscar a Simón ahora mismo",
        "Guardar la carta y seguir recopilando información antes de actuar",
        "Preguntarle a Robert directamente sobre su relación con Simón",
    ])
    if e == 1:
        G.decidir("revelar_carta_c3")
        narrar("El grupo reacciona con una mezcla de alivio y alarma. "
               "Lisa dice 'No está muerto' en voz muy baja, como si "
               "necesitara escucharse a sí misma decirlo para creerlo.")
        narrar("Si Simón está vivo, todo cambia. La urgencia une al grupo "
               "de una forma que las palabras no habían logrado.")
        for n in G.presentes(): G.p(n).conectar(15)
        G.guardar_pista("grupo_sabe_simon_vivo")
    elif e == 2:
        G.decidir("guardar_carta_c3")
        narrar("Guardas la carta en tu bolsillo. La información es poder, "
               "pero el tiempo que pasa es tiempo que Simón no tiene. "
               "Cada minuto de silencio es un minuto que alguien más "
               "puede usar para cubrir sus huellas.")
        G.guardar_pista("carta_guardada")
    else:
        G.decidir("confrontar_robert_c3")
        acotacion("Robert se queda en el umbral. Sus nudillos están blancos "
                  "en el marco de la puerta. No entra a la habitación.")
        dialogo("ROBERT", "Simón y yo... nuestra relación era complicada. "
                "Más de lo que nadie sabe. Y ahora que veo este lugar...")
        acotacion("No termina la frase. Baja la mirada hacia el suelo.")
        dialogo("ROBERT", "Ojalá hubiera sido diferente. Eso es todo lo "
                "que puedo decir.", False)
        G.p("Robert").conectar(20)
        G.guardar_pista("robert_relacion_complicada")

    _cierre_capitulo()


def explorar_habitacion():
    titulo("H A B I T A C I Ó N   D E   S I M Ó N", "Exploración libre")
    narrar("Una cama de madera tallada sin hacer, ropa doblada sobre una "
           "silla, una mesita de noche con un vaso de agua a medio beber "
           "todavía fresco. Hay un armario de espejo cuya puerta está "
           "entreabierta.")
    visto = set()
    while True:
        ops = []
        if "vaso" not in visto:
            ops.append("Examinar el vaso de agua en la mesita")
        if "mapa" not in visto:
            ops.append("Estudiar el mapa dibujado a mano en la pared")
        if "carta" not in visto:
            ops.append("Leer la carta a medio escribir en el suelo")
        if "cajon" not in visto:
            ops.append("Abrir el cajón de la mesita de noche")
        if "cama" not in visto:
            ops.append("Mirar bajo la cama")
        ops.append("Terminar de explorar")
        e = elegir(ops)
        sel = ops[e - 1]

        if "vaso" in sel:
            visto.add("vaso")
            narrar("El vaso tiene condensación fresca en el exterior. El agua "
                   "no ha tenido tiempo de evaporarse ni de calentarse.")
            escribir_lento("Simón estuvo aquí hace muy poco.")
            G.guardar_pista("vaso_condensacion")

        elif "mapa" in sel:
            visto.add("mapa")
            narrar("El plano de la mansión muestra todas las habitaciones. "
                   "Cuatro de ellas tienen una cruz al lado. Una quinta "
                   "habitación, en el ala norte, tiene un círculo con signo "
                   "de interrogación.")
            acotacion("El ala norte. Algo hay en el ala norte que Simón "
                      "marcó de forma diferente al resto.")
            G.guardar_pista("mapa_ala_norte")

        elif "carta" in sel:
            visto.add("carta")
            narrar("Una carta a medio escribir, sin destinatario. La pluma "
                   "está tirada en el suelo junto a la silla, como si alguien "
                   "la hubiera soltado con prisa. Dice:")
            dialogo("CARTA", "Si alguien lee esto, no estoy muerto. Estoy en...")
            narrar("La frase se corta. La frase inconclusa pesa en el cuarto "
                   "como una presencia física.")
            G.guardar_pista("simon_vivo")

        elif "cajón" in sel:
            visto.add("cajon")
            narrar("Dentro hay un frasco de pastillas con nombre de medicamento "
                   "para la ansiedad, una fotografía de un niño pequeño con el "
                   "nombre 'Lucas' escrito al dorso, y una llave pequeña sin "
                   "identificar.")
            acotacion("La fotografía del niño. El nombre Lucas. "
                      "¿Quién era ese niño para Simón?")
            G.guardar_pista("foto_nino_lucas")
            G.guardar_pista("llave_pequena")

        elif "cama" in sel:
            visto.add("cama")
            narrar("Un sobre de papel manila con los bordes quemados "
                   "parcialmente. Dentro hay una lista de nombres —ninguno "
                   "de los visitantes— junto a cifras y fechas. Al pie, "
                   "en letras mayúsculas:")
            dialogo("SOBRE", "TESTIGO.")
            acotacion("Simón fue testigo de algo. Y alguien intentó "
                      "quemar la evidencia sin lograrlo del todo.")
            G.guardar_pista("sobre_testigo")
        else:
            break


def interacciones_c3():
    titulo("H A B I T A C I Ó N", "Interacciones — Capítulo 3")
    hablados = set()
    while True:
        vivos = [n for n in G.presentes() if n not in hablados]
        if not vivos: break
        ops = [f"Hablar con {n}" for n in vivos] + ["No hablar con nadie más"]
        e = elegir(ops)
        if e > len(vivos): break
        nombre = vivos[e - 1]
        hablados.add(nombre)
        G.p(nombre).conectar(20)
        _dialogo_c3(nombre)


def _dialogo_c3(nombre):
    separador()
    if nombre == "Ben":
        acotacion("Ben tiene algo en las manos. Lo cierra de golpe, "
                  "demasiado rápido, cuando te acercas.")
        dialogo("BEN", "Estaba buscando algo que explicara qué pasó. "
                "Ya sabes, la agenda decía...")
        acotacion("Se interrumpe.")
        dialogo("BEN", "Mira, este cuarto me pone nervioso. El vaso de agua "
                "todavía tiene condensación. Eso significa...")
        acotacion("No termina. Deja lo que tenía sobre la cama con "
                  "cuidado excesivo.")
        dialogo("BEN", "¿Crees que sigue aquí?", False)

    elif nombre == "Lisa":
        acotacion("Lee la carta inconclusa en el suelo sin tocarla. "
                  "Se arrodilla para verla mejor y sus ojos recorren "
                  "las líneas dos veces, tres veces.")
        acotacion("Cuando se levanta, su expresión ha cambiado completamente: "
                  "ya no es la periodista calculadora que llegó al lobby.")
        dialogo("LISA", "No está muerto.")
        acotacion("Lo dice en voz muy baja.")
        dialogo("LISA", "Dios mío. No está muerto.", False)

    elif nombre == "Robert":
        acotacion("Lo dice mirando la habitación desde el umbral, sin entrar.")
        dialogo("ROBERT", "No debería estar aquí. Esta es la habitación de "
                "un hombre. Un espacio privado.")
        acotacion("Una pausa larga. Sus nudillos están blancos en el marco.")
        dialogo("ROBERT", "Simón y yo... nuestra relación era complicada. "
                "Más de lo que nadie sabe. Ojalá hubiera sido diferente. "
                "Eso es todo lo que puedo decir.", False)

    elif nombre == "Ana":
        dialogo("ANA", "El cajón de la mesita está entreabierto. La fotografía "
                "del niño. ¿La viste?")
        acotacion("Hay algo en su voz que es distinto a antes, más suave.")
        dialogo("ANA", "Simón nunca habló de su vida antes de ser Simón el "
                "pintor. Nunca. Como si hubiera decidido empezar a existir "
                "a los treinta y cinco años y punto.")
        dialogo("ANA", "Supongo que todos tenemos esa versión de nosotros "
                "que enterramos.", False)

    elif nombre == "Lucas":
        acotacion("Está mirando la fotografía del niño del cajón. No la toca.")
        dialogo("LUCAS", "Lucas. El nombre está escrito al dorso.")
        acotacion("Su voz es plana.")
        dialogo("LUCAS", "Hay un relicario que perteneció a Simón. Yo lo "
                "tengo, o lo tuve. Lo tomé sin permiso. Iba a devolverlo, "
                "siempre iba a devolverlo.")
        acotacion("Hace una pausa.")
        dialogo("LUCAS", "Si está aquí en algún lugar... si aparece entre "
                "sus cosas... necesito que no se asocie conmigo. ¿Entiendes "
                "lo que te estoy diciendo?", False)
    pausa()

# ══════════════════════════════════════════════════════════════
#  CAPÍTULO 4 — LA HORA DEL TALLER (7:00 PM)
# ══════════════════════════════════════════════════════════════

def capitulo_4():
    G.capitulo = 4
    titulo("C A P Í T U L O   4", "La Hora del Taller — 7:00 PM")
    time.sleep(0.5)

    narrar("Afuera ya casi no había luz. El ala sur de la mansión, donde "
           "estaba el taller, tenía los tragaluces del techo que ahora solo "
           "dejaban entrar la última luz cenicienta del día.")
    narrar("Alguien —ninguno de los cinco quiso decir quién— había encendido "
           "las luces interiores de la galería: unas tiras de luz fría que "
           "iluminaban los cuadros desde abajo, como en una exposición real.")
    pausa()

    explorar_galeria()
    interacciones_c4()

    titulo("D E C I S I Ó N", "Capítulo 4")
    if G.codigo_encontrado:
        narrar("Tienes el código: 4-7-2-9. El mapa de la habitación marcaba "
               "algo en el sótano. Lucas mencionó que cuatro dígitos podrían "
               "ser un código de acceso.")
    narrar("La noche se acerca. La mansión cambia cuando oscurece.")
    e = elegir([
        "Bajar al sótano a buscar la sala de seguridad" if G.codigo_encontrado
            else "Volver a la habitación de Simón a buscar más pistas",
        "Confrontar al grupo: exigir que todos digan la verdad",
        "Mantener al grupo unido y compartir todo lo descubierto",
    ])
    if e == 1:
        if G.codigo_encontrado:
            G.decidir("sotano_c4")
            narrar("El grupo baja al sótano. La puerta tiene un teclado "
                   "numérico. Introduces 4-7-2-9. Un clic. La puerta se abre.")
            G.guardar_pista("sala_camaras_abierta")
        else:
            G.decidir("volver_habitacion_c4")
            narrar("Vuelves a la habitación. Esta vez notas algo que antes "
                   "pasaste por alto: el mapa tiene una marca en el sótano.")
            G.guardar_pista("mapa_ala_norte")
    elif e == 2:
        G.decidir("confrontar_grupo_c4")
        narrar("El silencio que sigue a tu exigencia es el más largo de "
               "toda la tarde. Uno a uno, las máscaras empiezan a caer.")
        dialogo("BEN", "Hice cosas que no debería haber hecho. Cosas que "
                "tienen que ver con dinero y con la confianza que Simón me dio.")
        dialogo("ANA", "Las joyas. Necesito recuperar las joyas antes de "
                "que alguien descubra que estuvieron en mi poder.")
        acotacion("Robert no dice nada. Lucas mira al suelo. Lisa toma notas.")
        for n in G.presentes():
            G.p(n).aislar(10)
            G.p(n).conectar(5)
        G.guardar_pista("verdades_parciales")
    else:
        G.decidir("compartir_c4")
        narrar("El grupo se reúne en el centro de la galería, bajo el cuadro "
               "de los cinco visitantes. Por primera vez desde que llegaron, "
               "hablan de verdad. No de Simón. De ellos mismos.")
        narrar("No es una confesión completa. Pero es un comienzo. "
               "Y a veces un comienzo es suficiente para que la soledad "
               "retroceda un paso.")
        for n in G.presentes(): G.p(n).conectar(15)
        G.guardar_pista("grupo_comparte")

    _cierre_capitulo()


def explorar_galeria():
    titulo("L A   G A L E R Í A", "Exploración libre")
    narrar("Cuadros en distintos estados de terminación cubren las paredes. "
           "Hay manchas de pintura en el suelo, frascos de pigmento, trapos "
           "sucios. En el centro, una tela grande cubierta con una sábana "
           "blanca. El espacio huele a trementina y a algo metálico.")
    visto = set()
    while True:
        ops = []
        if "sabana" not in visto:
            ops.append("Levantar la sábana del cuadro central")
        if "grabador" not in visto:
            ops.append("Examinar el fonógrafo en la repisa")
        if "codigo" not in visto:
            ops.append("Examinar el cuadro abstracto con números en la firma")
        if "mancha" not in visto:
            ops.append("Investigar la mancha oscura cerca de la puerta trasera")
        if "lienzos" not in visto:
            ops.append("Buscar entre los lienzos apoyados en la pared")
        ops.append("Terminar de explorar")
        e = elegir(ops)
        sel = ops[e - 1]

        if "sábana" in sel:
            visto.add("sabana")
            narrar("Al levantar la sábana se revela un cuadro inacabado. "
                   "Representa a cinco personas de pie en un espacio que "
                   "claramente es el lobby de la mansión. Sus rostros están "
                   "apenas esbozados, pero sus posturas son identificables.")
            narrar("En el margen inferior, escrito con pintura fresca:")
            dialogo("CUADRO", "Los que llegaron a buscar.")
            acotacion("Simón los pintó. Antes de que llegaran. "
                      "Sabía que vendrían. Sabía quiénes serían.")
            G.guardar_pista("cuadro_cinco")

        elif "fonógrafo" in sel:
            visto.add("grabador")
            narrar("Un fonógrafo de mesa sobre la repisa, con un cilindro "
                   "de cera ya colocado. Al girar la manivela se escucha "
                   "la voz de Simón, distorsionada por el medio pero "
                   "inconfundible:")
            dialogo("SIMÓN", "Están en la casa. No sé cuántos. Pero no son "
                    "todos enemigos. Algunos solo tienen miedo.")
            acotacion("La voz es reciente. Cansada pero firme. "
                      "La voz de alguien que todavía tiene algo por lo que luchar.")
            G.guardar_pista("grabacion_simon")

        elif "números" in sel:
            visto.add("codigo")
            narrar("Uno de los cuadros terminados, una composición abstracta, "
                   "tiene una serie de números escritos en el ángulo inferior "
                   "derecho que parecen parte de la firma.")
            narrar("Los números son: 4 - 7 - 2 - 9")
            acotacion("No son números de catálogo. Son demasiado cortos. "
                      "Cuatro dígitos. Un código.")
            G.guardar_pista("codigo_4729")
            G.codigo_encontrado = True

        elif "mancha" in sel:
            visto.add("mancha")
            narrar("Cerca de la puerta trasera del taller hay una mancha "
                   "oscura en el suelo de madera. No es pequeña. Se extiende "
                   "en un arco irregular, como si alguien hubiera sido "
                   "arrastrado. Un trapo sucio la cubre parcialmente, "
                   "como si alguien intentó limpiarla con prisa y se rindió.")
            narrar("Te agachas. El olor te golpea antes de que puedas "
                   "procesarlo: cobre, sal, algo orgánico que el cuerpo "
                   "reconoce antes que la mente. La mancha todavía está "
                   "húmeda en el centro. Fresca. De horas, no de días.")
            escribir_miedo("Esto no pasó hace una semana. Esto pasó hoy. "
                           "Alguien sangró aquí mientras ustedes exploraban "
                           "el lobby.")
            G.guardar_pista("mancha_sangre")

        elif "lienzos" in sel:
            visto.add("lienzos")
            narrar("Oculta entre varios lienzos apoyados en la pared hay "
                   "una carpeta de plástico transparente con fotografías "
                   "y documentos. Las fotos muestran el exterior de un "
                   "almacén en llamas. Los documentos son copias de "
                   "declaraciones juradas.")
            acotacion("La evidencia que Lisa vino a buscar. Está aquí, "
                      "escondida entre el arte de Simón.")
            G.guardar_pista("carpeta_lisa")
        else:
            break


def interacciones_c4():
    titulo("L A   G A L E R Í A", "Interacciones — Capítulo 4")
    hablados = set()
    while True:
        vivos = [n for n in G.presentes() if n not in hablados]
        if not vivos: break
        ops = [f"Hablar con {n}" for n in vivos] + ["No hablar con nadie más"]
        e = elegir(ops)
        if e > len(vivos): break
        nombre = vivos[e - 1]
        hablados.add(nombre)
        G.p(nombre).conectar(20)
        _dialogo_c4(nombre)


def _dialogo_c4(nombre):
    separador()
    if nombre == "Robert":
        acotacion("Está frente al cuadro descubierto, estudiando las figuras. "
                  "Se reconoce a sí mismo en la del fondo, la más retirada.")
        dialogo("ROBERT", "Nos conocía bien. Demasiado bien.")
        acotacion("Se gira hacia ti.")
        dialogo("ROBERT", "Hay una carta. En algún lugar de esta casa hay "
                "una carta que no debería existir o que, si existe, debería "
                "haber llegado a mí hace años. Si la encuentras...", False)
        acotacion("No termina la petición. Pero el significado es claro.")

    elif nombre == "Ana":
        dialogo("ANA", "Ese cuadro lo empezó hace dos meses. Lo reconozco "
                "por la preparación de la tela, es su técnica de los últimos "
                "años. Dos meses atrás, Simón estaba vivo y pintando.")
        acotacion("Te mira directamente.")
        dialogo("ANA", "Alguien mintió sobre su muerte. La pregunta es si "
                "ese alguien está en esta habitación con nosotros ahora mismo.", False)

    elif nombre == "Ben":
        acotacion("Ben ha encontrado el fonógrafo antes que tú. Cuando llegas, "
                  "está inclinado sobre la bocina con los ojos cerrados, "
                  "escuchando. Al darse cuenta de que no está solo, levanta la aguja.")
        dialogo("BEN", "Era su voz. La grabación. Era él.")
        acotacion("Traga saliva.")
        dialogo("BEN", "Mira, yo hice cosas que no debería haber hecho. "
                "Cosas que tienen que ver con dinero y con la confianza que "
                "Simón me dio. Y si está vivo... hay cosas que tendría que "
                "explicarle. Cosas que no sé cómo explicar.", False)

    elif nombre == "Lisa":
        dialogo("LISA", "La mancha del suelo, cerca de la puerta trasera. "
                "Eso no es pintura. Conozco la diferencia.")
        acotacion("Saca una libreta y dibuja un croquis rápido de la escena.")
        dialogo("LISA", "Simón fue atacado aquí. O llevado desde aquí. "
                "Lo que significa que quien lo tiene está en la casa y "
                "probablemente sabe que estamos buscando. Necesitamos "
                "encontrarlo antes de que oscurezca del todo.", False)

    elif nombre == "Lucas":
        acotacion("Lucas está mirando un cuadro: un retrato de un niño "
                  "pequeño que mira hacia arriba. El niño tiene el mismo "
                  "relicario de plata en el cuello.")
        dialogo("LUCAS", "Hay un cuadro con una serie de números en la firma. "
                "Lo vi hace un momento. No son números de catálogo. Son "
                "demasiado cortos. Cuatro dígitos.")
        dialogo("LUCAS", "¿Para qué necesitaría Simón un código de cuatro "
                "dígitos en un espacio como este?", False)
    pausa()

# ══════════════════════════════════════════════════════════════
#  CAPÍTULO 5 — LA NOCHE (10:00 PM)
# ══════════════════════════════════════════════════════════════

def capitulo_5():
    G.capitulo = 5
    titulo("C A P Í T U L O   5", "La Noche — 10:00 PM")
    time.sleep(0.5)

    narrar("La mansión a las diez de la noche era un organismo distinto. "
           "Los ruidos de la madera al asentarse, el viento que encontraba "
           "grietas en las ventanas mal selladas del pasillo norte, el "
           "zumbido sordo de los equipos electrónicos en el sótano.")
    n_vivos = len(G.presentes())
    if n_vivos == 5:
        narrar("El grupo había permanecido unido más tiempo del que "
               "cualquiera habría predicho. Nadie lo decía, pero todos "
               "lo sentían: en conjunto eran más seguros que solos.")
    elif n_vivos >= 3:
        narrar("El grupo se había reducido. Los que quedaban se miraban "
               "con una mezcla de desconfianza y necesidad. Cada ausencia "
               "pesaba como una acusación silenciosa.")
    else:
        narrar("Quedaban pocos. La mansión se sentía más grande con cada "
               "persona que se iba. Los pasillos eran más largos, las "
               "sombras más densas, el silencio más absoluto.")
    pausa()

    # ── Sala de cámaras ──
    if G.codigo_encontrado or G.tiene("sala_camaras_abierta"):
        explorar_sala_camaras()
    else:
        titulo("E L   S Ó T A N O")
        narrar("Bajas al sótano. Hay una puerta con un teclado numérico. "
               "Sin el código, no puedes entrar.")
        e = elegir([
            "Intentar combinaciones al azar",
            "Volver arriba y buscar el código",
            "Forzar la puerta",
        ])
        if e == 1:
            narrar("Pruebas varias combinaciones. Ninguna funciona. "
                   "Pero al tercer intento, recuerdas algo: el cuadro "
                   "abstracto en la galería tenía números en la firma.")
            e2 = elegir(["Ir a la galería a buscar el código", "Rendirse"])
            if e2 == 1:
                narrar("Vuelves a la galería. Los números: 4-7-2-9. "
                       "Corres al sótano. El código funciona.")
                G.codigo_encontrado = True
                G.guardar_pista("codigo_4729")
                explorar_sala_camaras()
            else:
                narrar("Sin la sala de cámaras, tendrás que buscar a Simón "
                       "a ciegas en el ala norte.")
        elif e == 2:
            narrar("Subes a la galería. El cuadro abstracto tiene los números "
                   "4-7-2-9 en la firma. Vuelves al sótano. Funciona.")
            G.codigo_encontrado = True
            G.guardar_pista("codigo_4729")
            explorar_sala_camaras()
        else:
            narrar("La puerta no cede. Está diseñada para resistir. "
                   "Tendrás que buscar a Simón sin la información "
                   "de las cámaras.")

    # ── Los objetos ──
    titulo("L O S   O B J E T O S", "Cada uno encuentra lo que vino a buscar")
    narrar("Antes de ir al ala norte, cada uno tiene una última oportunidad "
           "de encontrar lo que vino a buscar. Ayudarlos fortalece el vínculo. "
           "Ignorarlos los empuja más cerca del borde.")
    for nombre in list(G.presentes()):
        p = G.p(nombre)
        if p.objeto_encontrado:
            continue
        separador()
        print(f"  {nombre.upper()} busca: {p.objeto}")
        print(f"  Ubicación: {p.ubicacion_obj}")
        e = elegir([
            f"Ayudar a {nombre} a encontrar su objeto",
            "Dejarlo por su cuenta",
        ])
        if e == 1:
            p.conectar(20)
            p.objeto_encontrado = True
            _encontrar_objeto(nombre)
        else:
            p.aislar(15)
            acotacion(f"{nombre} te mira un momento. Asiente despacio. "
                      f"Se aleja solo hacia {p.ubicacion_obj.lower()}.")

    # ── El rescate ──
    el_rescate()

    G.fin_capitulo()
    abandonado = G.verificar_abandonos()
    if abandonado:
        _escena_abandono(abandonado)


def explorar_sala_camaras():
    titulo("S A L A   D E   V I G I L A N C I A", "Sótano de la mansión")
    narrar("Una habitación pequeña y sin ventanas. Las paredes están "
           "cubiertas de espejos angulados y mirillas que conectan con "
           "cada habitación de la casa mediante un sistema de tubos y "
           "cristales. Un mecanismo ingenioso, casi paranoico.")
    narrar("Hay una silla de madera frente al panel de mirillas, y en "
           "ella, abandonado, un maletín de cuero negro. Las mirillas "
           "que aún no están obstruidas muestran pasillos vacíos y una "
           "habitación con una figura sentada, apenas visible.")
    visto = set()
    while True:
        ops = []
        if "monitor" not in visto:
            ops.append("Mirar por la mirilla central")
        if "maletin" not in visto:
            ops.append("Abrir el maletín de cuero negro")
        if "disco" not in visto:
            ops.append("Revisar los cilindros de fonógrafo almacenados")
        if "nota_roja" not in visto:
            ops.append("Leer la nota con tinta roja clavada en la pared")
        if "mapa_nuevo" not in visto:
            ops.append("Estudiar el mapa actualizado")
        ops.append("Continuar")
        e = elegir(ops)
        sel = ops[e - 1]

        if "mirilla central" in sel:
            visto.add("monitor")
            narrar("A través de la mirilla se distingue una figura humana "
                   "sentada en una silla en una habitación que podría ser "
                   "cualquiera de las cinco del ala norte. La figura no se "
                   "mueve. No es posible determinar si está atada o "
                   "simplemente inmóvil.")
            G.guardar_pista("figura_monitor")

        elif "maletín" in sel:
            visto.add("maletin")
            narrar("Dentro del maletín negro hay, entre otros objetos, un "
                   "relicario de plata antiguo. En su interior, grabado:")
            dialogo("RELICARIO", "Para Lucas. Siempre.")
            G.guardar_pista("relicario_encontrado")
            G.p("Lucas").objeto_encontrado = True
            if "Lucas" in G.presentes():
                G.p("Lucas").conectar(20)

        elif "cilindros" in sel:
            visto.add("disco")
            narrar("Hay una docena de cilindros de cera etiquetados con "
                   "fechas de los últimos seis meses. El más reciente, de "
                   "hace dos días, al reproducirlo en el fonógrafo del "
                   "rincón, se escucha a Simón hablando con alguien. "
                   "Simón dice:")
            glitch(intensidad=2)
            dialogo_roto("SIMÓN", "No voy a callar más. Aunque eso me cueste la vida.")
            G.guardar_pista("grabacion_amenaza")

        elif "nota" in sel:
            visto.add("nota_roja")
            narrar("Escrita con marcador rojo sobre un papel:")
            dialogo("NOTA", "Si encuentras esto, ya saben que estás aquí. "
                    "No uses las luces del pasillo norte.")
            G.guardar_pista("advertencia_norte")

        elif "mapa" in sel:
            visto.add("mapa_nuevo")
            narrar("Una versión más reciente del mapa dibujado a mano. "
                   "Una habitación del ala norte está marcada con una X roja "
                   "y la palabra 'aquí' escrita al lado. Es la misma "
                   "habitación que se ve por la mirilla.")
            G.guardar_pista("ubicacion_simon")
        else:
            break


def _encontrar_objeto(nombre):
    if nombre == "Ben":
        narrar("Ben encuentra el sobre con efectivo y el libro de cuentas "
               "en la habitación de Simón, donde Simón lo había dejado "
               "para que Ben lo encontrara.")
        narrar("Dentro del sobre hay una nota de puño y letra del pintor:")
        dialogo("SIMÓN (nota)", "Ya lo sé. Siempre lo supe. Pero confié "
                "en que lo arreglarías.")
        acotacion("Ben tardó un momento en poder moverse después de leerla. "
                  "Sus ojos se llenaron de algo que no era miedo. Era vergüenza.")

    elif nombre == "Lisa":
        narrar("Lisa encuentra la carpeta entre los lienzos de la galería. "
               "Las fotografías, los documentos, las declaraciones juradas: "
               "todo está ahí. La evidencia que necesitaba para terminar "
               "lo que había empezado.")
        acotacion("La guarda sin decir nada. Pero sus manos tiemblan.")

    elif nombre == "Robert":
        narrar("Robert encuentra el sobre con su nombre en el cajón de la "
               "cómoda del lobby. Lo abre con manos que no tiemblan porque "
               "había decidido no dejarlas temblar.")
        narrar("Lee la carta del padre dos veces. La dobla y se la mete "
               "en el bolsillo interior del saco.")
        acotacion("No dice nada. Pero algo en su postura cambia, como si "
                  "un peso que llevaba décadas cargando se hubiera movido "
                  "de un hombro al otro.")

    elif nombre == "Ana":
        if G.tiene("llave_pequena"):
            narrar("Ana usa la llave pequeña del cajón de la mesita de Simón. "
                   "Encaja perfectamente en el archivador del estudio. "
                   "El estuche de cuero está adentro. Las joyas están intactas.")
        else:
            narrar("Ana encuentra el estuche de cuero en el cajón cerrado "
                   "del estudio. Alguien lo había dejado entreabierto.")
        acotacion("Las guarda sin decir nada. Pero sus ojos se cierran "
                  "un momento, como quien exhala después de contener "
                  "la respiración demasiado tiempo.")

    elif nombre == "Lucas":
        if G.tiene("relicario_encontrado"):
            narrar("Lucas ya tiene el relicario. Lo sostiene en la palma "
                   "de la mano, pasando el pulgar por la inscripción.")
        else:
            narrar("Lucas encuentra el relicario en el maletín de la sala "
                   "de vigilancia. Lo toma con ambas manos, lo abre y lee "
                   "la inscripción interior: 'Para Lucas. Siempre.'")
        acotacion("Lo cierra. Lo guarda. Toma una decisión silenciosa "
                  "que nadie más ve.")
    pausa()

# ══════════════════════════════════════════════════════════════
#  EL RESCATE
# ══════════════════════════════════════════════════════════════

def el_rescate():
    titulo("E L   R E S C A T E", "Ala Norte — Pasillo oscuro")
    narrar("El pasillo norte era exactamente como la nota advertía: largo, "
           "oscuro, con el tipo de silencio que no es ausencia de sonido "
           "sino presencia de algo que aguarda.")
    if G.tiene("advertencia_norte"):
        narrar("No usan las luces. Encienden las velas que encontraron "
               "en el lobby, protegiendo la llama con la mano.")
    else:
        narrar("Encienden las luces del pasillo. El fluorescente parpadea "
               "dos veces antes de estabilizarse.")
        narrar("Entonces se apagan. Todas. De golpe. Oscuridad absoluta. "
               "El tipo de oscuridad que tiene peso, que se siente en la "
               "piel como algo húmedo y vivo.")
        narrar("Alguien del grupo contiene la respiración. En el silencio "
               "que sigue, un sonido: respiración. Pero no es de nadie "
               "del grupo. Viene de más adelante en el pasillo. Lenta. "
               "Profunda. Como la de alguien que lleva mucho tiempo "
               "esperando en la oscuridad y acaba de escuchar que "
               "su espera terminó.")
        acotacion("Quien los trajo aquí no quiere que salgan fácilmente.")
        for n in G.presentes(): G.p(n).aislar(10)

    narrar("Hay cinco puertas. Todas cerradas. Todas iguales.")
    pausa()

    if G.tiene("ubicacion_simon"):
        narrar("El mapa actualizado señala la tercera puerta con una X roja "
               "y la palabra 'aquí'. Pero en la oscuridad, frente a cinco "
               "puertas idénticas, la certeza se vuelve más frágil.")
    e = elegir([
        "Abrir la primera puerta (más cercana a la escalera)",
        "Abrir la tercera puerta (la que señala el mapa)" if G.tiene("ubicacion_simon")
            else "Abrir la tercera puerta (intuición)",
        "Abrir la quinta puerta (al fondo del pasillo)",
    ])

    if e == 2:
        G.simon_encontrado = True
        _escena_rescate_directo()
    elif e == 1:
        narrar("La primera habitación está vacía. Una silla, una mesa, "
               "nada más. Pero en la mesa hay una nota:")
        dialogo("NOTA", "Más adelante.")
        narrar("Abres la siguiente puerta. Y la siguiente.")
        G.simon_encontrado = True
        _escena_rescate_directo()
        acotacion("Perdiste tiempo. Pero lo encontraste.")
        for n in G.presentes(): G.p(n).aislar(5)
    else:
        narrar("Al fondo del pasillo, la quinta puerta. La abres.")
        narrar("No es Simón.")
        time.sleep(0.8)
        narrar("La habitación está vacía excepto por una silla volcada "
               "y un espejo roto en la pared opuesta. En los fragmentos "
               "del espejo, por un instante, ves un reflejo que no es "
               "el tuyo. Una silueta detrás de ti. Más alta. Inmóvil. "
               "Con algo en la mano que brilla bajo la luz de la linterna.")
        time.sleep(0.5)
        narrar("Te giras.")
        time.sleep(0.8)
        narrar("No hay nadie.")
        time.sleep(0.5)
        narrar("Pero en el suelo, justo donde estaba la silueta, hay "
               "huellas frescas en el polvo. Y algo más: gotas. Pequeñas. "
               "Oscuras. Todavía húmedas. Forman un rastro que sale de "
               "la habitación y se pierde en la oscuridad del pasillo. "
               "Hacia donde está el grupo.")
        acotacion("Quien estuvo aquí hace segundos ahora está entre "
                  "ustedes y la escalera. Entre ustedes y la salida.")
        narrar("El grupo retrocede pegado a la pared. Abren la tercera puerta.")
        G.simon_encontrado = True
        _escena_rescate_directo()
        for n in G.presentes(): G.p(n).aislar(10)


def _escena_rescate_directo():
    separador()
    escribir_lento("La puerta se abre.")
    pantalla_negra(1)
    limpiar()
    print()
    time.sleep(0.5)
    narrar("Simón está sentado en una silla, con las manos atadas con una "
           "cuerda fina al respaldo, una venda manchada en la frente. "
           "Tiene los labios agrietados y los ojos hundidos. Cuando la luz "
           "de las linternas lo alcanza, su cuerpo se tensa como el de un "
           "animal acorralado.")
    time.sleep(0.5)
    narrar("Tarda un momento en enfocar. En entender que no es una amenaza. "
           "Que son personas. Que vinieron a buscarlo.")
    time.sleep(0.3)
    dialogo_roto("SIMÓN", "...agua.")
    acotacion("Es lo primero que dice. No un nombre. No una explicación. "
              "La necesidad más básica de un cuerpo que lleva días sin cuidado.")
    time.sleep(0.5)

    n_vivos = len(G.presentes())
    if n_vivos == 5:
        narrar("Cuando sus ojos se acostumbraron a la luz y pudo distinguir "
               "los rostros, algo en su cuerpo cambió. Los hombros cayeron. "
               "La mandíbula se relajó. Como si un músculo que llevaba días "
               "tenso decidiera por fin soltarse.")
        dialogo_roto("SIMÓN", "...todos.")
        acotacion("Lo dice casi sin voz. Una sola palabra que contiene "
                  "más alivio del que cualquier discurso podría expresar.")
    elif n_vivos >= 3:
        narrar("Simón contó los rostros con la mirada. Su expresión cambió "
               "levemente al notar las ausencias.")
        dialogo_roto("SIMÓN", "¿Los demás...?")
        acotacion("No termina la pregunta. No tiene fuerzas. "
                  "Pero la preocupación es genuina.")
    else:
        narrar("Simón miró los pocos rostros frente a él. "
               "Algo en sus ojos se apagó un momento.")
        dialogo_roto("SIMÓN", "...¿solo...?")
        acotacion("No puede terminar. Cierra los ojos un momento.")
    pausa()

    # ── Escena del rescate según quién está ──
    vivos = G.presentes()
    if "Ben" in vivos:
        narrar("Ben fue el primero en arrodillarse para desatar las cuerdas. "
               "Sus manos no temblaban, aunque quizás debían hacerlo.")
    if "Robert" in vivos:
        narrar("Robert se mantuvo de pie en el umbral, rígido, pero no se fue.")
    if "Ana" in vivos:
        narrar("Ana sostuvo la linterna para que pudieran ver mejor el nudo.")
    if "Lisa" in vivos:
        narrar("Lisa sacó su libreta y dibujó un croquis de la habitación, "
               "metódica incluso en ese momento.")
    if "Lucas" in vivos:
        narrar("Lucas fue el último en entrar al cuarto, y cuando lo hizo, "
               "sacó el relicario del bolsillo y lo colocó con cuidado "
               "sobre la pequeña mesa junto a la silla de Simón, sin decir nada.")
        time.sleep(0.5)
        narrar("Simón miró el relicario. Luego miró a Lucas. "
               "Sus ojos se llenaron de algo que no era rabia. "
               "Era reconocimiento. El tipo de reconocimiento que solo "
               "existe entre dos personas que comparten un secreto.")
        time.sleep(0.3)
        dialogo_roto("SIMÓN", "...el relicario.")
        acotacion("Lo dice tocándolo con los dedos entumecidos. "
                  "Luego mira a Lucas una vez más.")
        dialogo_roto("SIMÓN", "...está bien.", False)
        acotacion("Dos palabras. Dichas con el esfuerzo de alguien que "
                  "apenas puede hablar. Pero que eligió usar lo que le "
                  "queda de voz para decir exactamente eso.")
    pausa()

    # ── Decisión final ──
    titulo("L A   Ú L T I M A   D E C I S I Ó N")
    narrar("Simón está libre. Las cuerdas están en el suelo. "
           "El grupo —lo que queda de él— está reunido en el pasillo norte.")
    narrar("Nadie preguntó qué había pasado exactamente. No esa noche. "
           "Había tiempo para las preguntas, para las explicaciones, "
           "para los secretos que cada uno tendría que decidir si revelaba "
           "o guardaba.")
    escribir_lento("Esa noche solo había que salir de la mansión.")
    pausa()

    narrar("Pero antes de salir, una última decisión.")
    e = elegir([
        "Salir todos juntos ahora mismo, sin mirar atrás",
        "Quedarse un momento más — dejar que cada uno se despida del lugar",
        "Buscar al asesino antes de irse",
    ])
    G.decidir(f"final_c5_{e}")

    if e == 1:
        G.decidir("salir_juntos")
        narrar("El grupo camina hacia la puerta principal. Nadie habla. "
               "Los pasos de todos suenan al unísono sobre la madera vieja.")
        for n in G.presentes(): G.p(n).conectar(10)
    elif e == 2:
        G.decidir("despedida")
        narrar("Cada uno se toma un momento. Ben mira el estudio una última "
               "vez. Robert toca el marco de la puerta del lobby como "
               "despidiéndose de algo que nunca tuvo. Ana recorre la galería "
               "con los ojos. Lisa guarda su libreta sin escribir nada. "
               "Lucas mira la fachada desde la ventana.")
        for n in G.presentes(): G.p(n).conectar(15)
    else:
        G.decidir("buscar_asesino")
        narrar("Recorren la mansión. Cada habitación, cada pasillo, cada "
               "rincón. En la galería, la puerta trasera está abierta de "
               "par en par. Hay marcas de barro fresco en el umbral y "
               "algo más: un cuchillo de cocina en el suelo, con la hoja "
               "manchada de algo oscuro que ya se secó.")
        narrar("En el estudio, el cajón del archivador que estaba cerrado "
               "ahora está abierto y vacío. En el lobby, la fotografía "
               "del grupo —la que apareció con la X de sangre— ya no está. "
               "Alguien se la llevó como recuerdo.")
        narrar("No encuentran a nadie. Pero en la última habitación que "
               "revisan, la del ala norte donde estaba Simón, hay algo "
               "nuevo escrito en la pared con el mismo rojo de siempre:")
        dialogo("PARED", "Volveré cuando se olviden de tener miedo.")
        acotacion("El asesino se fue. Pero dejó claro que esto no terminó.")
        for n in G.presentes(): G.p(n).aislar(5)

# ══════════════════════════════════════════════════════════════
#  EL FINAL
# ══════════════════════════════════════════════════════════════

def determinar_final():
    """Nadie sale vivo. Lo que cambia es cuánto saben antes de morir."""
    vivos = G.presentes()
    n_vivos = len(vivos)
    muertos = [nm for nm, p in G.personajes.items() if not p.presente]
    n_muertos = len(muertos)

    # ── ACTO 1: La falsa salida ──
    pantalla_negra(2)

    if G.simon_encontrado and n_vivos >= 3:
        titulo("L A   S A L I D A")
        narrar("Caminaron hacia la puerta principal. Simón iba entre ellos, "
               "apoyado en alguien, arrastrando los pies. Nadie habló. "
               "Los pasos sonaban huecos en la madera, como latidos de un "
               "corazón que se apaga.")
        if n_muertos > 0:
            narrar(f"Pasaron junto a los lugares donde encontraron a "
                   f"{', '.join(muertos)}. Nadie miró. Nadie quiso "
                   f"confirmar que los cuerpos seguían ahí.")
        narrar("La puerta principal estaba abierta. El aire de la noche "
               "entró como algo vivo, frío, que olía a tierra mojada "
               "y a libertad.")
        narrar("Dieron un paso afuera. Dos. Tres.")
        time.sleep(0.5)
        narrar("Entonces la puerta se cerró detrás de ellos.")
        time.sleep(0.5)
        narrar("No. No se cerró. Alguien la cerró. Desde adentro.")
        time.sleep(0.5)
        narrar("Y las luces del camino de tierra se apagaron una a una, "
               "como velas que alguien sopla con paciencia, dejándolos "
               "en una oscuridad que no era la de la noche. Era más "
               "densa. Más hambrienta.")
    elif G.simon_encontrado and n_vivos >= 1:
        titulo("L O   Q U E   Q U E D A")
        if n_vivos == 1:
            nombre = vivos[0]
            narrar(f"Solo quedaban {nombre} y Simón. Dos personas en una "
                   f"mansión que olía a sangre y a barniz viejo. Los demás "
                   f"estaban muertos. Cada uno en su rincón.")
        else:
            narrar(f"Solo quedaban {', '.join(vivos)} y Simón. Los que "
                   f"faltaban estaban repartidos por la mansión como "
                   f"ofrendas que nadie pidió.")
        narrar("Caminaron hacia la puerta. Simón apenas podía sostenerse.")
        time.sleep(0.5)
        narrar("La puerta estaba cerrada. Con llave. Desde afuera.")
        narrar("Simón no pareció sorprendido.")
    else:
        titulo_horror("L A   M A N S I Ó N   G A N Ó")
        narrar("Nadie encontró a Simón. El grupo se deshizo antes de "
               "llegar al ala norte. Uno a uno, el aislamiento los separó "
               "y la oscuridad hizo el resto.")
        narrar("Cinco personas entraron buscando algo. Cinco cuerpos "
               "quedaron repartidos por la mansión como piezas de un "
               "rompecabezas que nadie armó.")
    pausa()

    # ── ACTO 2: La última noche ──
    if G.simon_encontrado and n_vivos >= 1:
        pantalla_negra(1.5)
        titulo("L A   Ú L T I M A   N O C H E")
        time.sleep(1)
        narrar("No pudieron salir. Las ventanas estaban selladas desde "
               "afuera con tablas que no estaban ahí cuando llegaron. "
               "Los coches tenían las ruedas reventadas. El camino de "
               "tierra terminaba en un derrumbe que no existía esa tarde.")
        narrar("Alguien había preparado todo mientras ellos buscaban "
               "a Simón. Alguien que conocía cada minuto de su recorrido.")
        pausa()
        if n_vivos >= 3:
            narrar("Se reunieron en el lobby. Encendieron todas las velas "
                   "que encontraron. Se sentaron en los sillones de cuero "
                   "rojo, los mismos donde se sentaron esa tarde cuando "
                   "todavía eran cinco desconocidos con secretos.")
            narrar("Nadie durmió. Nadie habló. Solo esperaron.")
            time.sleep(0.5)
            narrar("A las tres de la mañana, las velas se apagaron. "
                   "Todas. Al mismo tiempo. Como si alguien hubiera "
                   "aspirado el aire de la habitación.")
            time.sleep(0.8)
            beep()
            glitch(intensidad=3)
            narrar("En la oscuridad, pasos. No de afuera. De adentro. "
                   "De las paredes. Del techo. Del suelo bajo sus pies. "
                   "Como si la mansión entera se hubiera puesto en "
                   "movimiento.")
        else:
            nombre = vivos[0]
            narrar(f"{nombre} intentó forzar una ventana. El cristal no "
                   f"cedió. Golpeó con una silla. Nada. El cristal "
                   f"absorbía los golpes como si fuera blando.")
            narrar("Simón se sentó en el suelo del lobby. No intentó "
                   "ayudar. No intentó escapar. Se quedó quieto, con "
                   "la mirada fija en la escalera, como si esperara "
                   "a alguien que sabía que iba a bajar.")
            time.sleep(0.5)
            dialogo_roto("SIMÓN", "...ya viene.")
        pausa()

        # ── La muerte de los sobrevivientes ──
        pantalla_negra(2)
        beep()
        titulo_horror("A M A N E C E R")
        time.sleep(1.5)
        narrar("La policía llegó a la mansión tres días después, "
               "siguiendo una denuncia anónima. La puerta principal "
               "estaba abierta de par en par. Las tablas de las ventanas "
               "habían desaparecido. El derrumbe del camino no existía.")
        narrar("Como si nada hubiera pasado.")
        pausa()
        narrar("Encontraron los cuerpos.")
        time.sleep(1)
        if n_muertos > 0:
            narrar(f"Primero los de {', '.join(muertos)}, en los lugares "
                   f"donde el grupo los había encontrado. Exactamente "
                   f"como los dejaron. Intactos.")
        for nombre in vivos:
            time.sleep(0.5)
            _muerte_final(nombre)
        # Simón
        time.sleep(1)
        narrar("Y a Simón.")
        time.sleep(0.5)
        narrar("Sentado en el escalón de la entrada. Con las muñecas "
               "todavía enrojecidas por las cuerdas. Los ojos abiertos. "
               "Mirando hacia el camino de tierra como si esperara "
               "a alguien que nunca llegó.")
        narrar("Tenía un pincel en la mano derecha. Mojado en pintura "
               "roja. Y en el suelo, a sus pies, un cuadro pequeño "
               "recién terminado.")
        narrar("El cuadro mostraba seis personas sentadas en un lobby. "
               "Todas muertas. Todas con los ojos abiertos. Todas "
               "mirando al espectador.")
        time.sleep(0.5)
        narrar("En el margen inferior, con la misma pintura roja:")
        texto_sangre("Los que llegaron a buscar.")
        pausa()

    # ── ACTO 3: La verdad ──
    pantalla_negra(2)
    beep()
    titulo_horror("L A   V E R D A D")
    time.sleep(1.5)
    narrar("La investigación de Lisa —lo que quedaba de ella en su "
           "libreta manchada— conectó las piezas. La carpeta de la "
           "galería. El libro de contabilidad. La carta del padre. "
           "Las joyas. El relicario.")
    narrar("Cada objeto que los cinco vinieron a buscar había sido "
           "colocado deliberadamente en la mansión. En el lugar exacto "
           "donde cada uno lo encontraría. Como cebo en una trampa "
           "diseñada con la paciencia de alguien que conoce a sus "
           "presas mejor de lo que ellas se conocen a sí mismas.")
    pausa()
    narrar("La noticia de la muerte de Simón fue falsa. Enviada desde "
           "una oficina de correos a las afueras de la ciudad, tres "
           "días antes de la reunión. El sobre tenía huellas. Las "
           "huellas coincidían con las de Simón.")
    time.sleep(0.5)
    escribir_lento("Simón envió la noticia de su propia muerte.")
    time.sleep(1)
    escribir_lento("Simón preparó los objetos.")
    time.sleep(0.5)
    escribir_lento("Simón los atrajo a la mansión.")
    time.sleep(1.5)
    pausa()
    narrar("Pero Simón no se ató a sí mismo a una silla en el ala norte. "
           "Alguien más hizo eso. Alguien que Simón invitó a la mansión "
           "antes que a los cinco. Alguien que llegó primero.")
    time.sleep(0.5)
    narrar("La sexta persona de la fotografía.")
    time.sleep(0.5)
    narrar("La del rostro cubierto con cinta negra.")
    time.sleep(1)
    if G.tiene("nota_abrigo"):
        narrar("La nota en el abrigo del perchero. 'No confíes en nadie "
               "que llegue antes que tú.' No era una advertencia de Simón "
               "para los visitantes.")
        escribir_lento("Era una advertencia de Simón para sí mismo.")
        escribir_lento("Una advertencia que no siguió.")
    pausa()

    # ── ACTO 4: El ciclo ──
    pantalla_negra(2)
    glitch(intensidad=4)
    time.sleep(0.5)
    limpiar()
    print()
    time.sleep(1)
    narrar("El caso fue archivado. Seis muertes en una propiedad rural. "
           "Circunstancias por determinar. Sin sospechosos. Sin testigos. "
           "Sin explicación.")
    narrar("La mansión fue clausurada. Las puertas selladas. Las ventanas "
           "tapiadas. Un cartel de 'Prohibido el paso' clavado en la reja.")
    pausa()
    narrar("Seis meses después, el cartel había desaparecido.")
    time.sleep(0.5)
    narrar("Las puertas estaban abiertas.")
    time.sleep(0.5)
    narrar("Y cinco personas recibieron invitaciones.")
    time.sleep(1)
    narrar("Cinco sobres. Cinco nombres nuevos. Cinco razones para ir "
           "a una mansión antigua en las afueras de la ciudad.")
    narrar("Las invitaciones estaban escritas a mano. Con una letra "
           "que nadie reconoció. Pero que, si alguien hubiera comparado "
           "con la nota del tablón del estudio, habría resultado "
           "idéntica.")
    time.sleep(0.5)
    escribir_lento("La letra de Simón.")
    time.sleep(0.5)
    escribir_lento("Un hombre que llevaba tres meses muerto.")
    time.sleep(1)
    beep()
    glitch(intensidad=5)
    narrar("La mansión los esperaba. Limpia. Ordenada. Con flores "
           "frescas en el jarrón del lobby y un abrigo colgado en "
           "el perchero.")
    time.sleep(0.5)
    narrar("En el bolsillo interior del abrigo, una nota manuscrita "
           "sin firma:")
    time.sleep(0.5)
    texto_sangre("No confíes en nadie que llegue antes que tú.")
    time.sleep(1)
    narrar("La misma nota.")
    time.sleep(0.3)
    narrar("La misma mansión.")
    time.sleep(0.3)
    narrar("La misma trampa.")
    time.sleep(1.5)
    pantalla_negra(1)
    limpiar()
    print("\n" * 3)
    time.sleep(1)
    escribir_lento("Nuevos nombres.")
    time.sleep(0.5)
    escribir_lento("Nuevos secretos.")
    time.sleep(1)
    escribir_lento("El mismo final.")
    time.sleep(2)
    print(f"\n\n{'F I N':^{W}}\n")
    time.sleep(1)
    print(f"\033[2m{'La Mansión de Simón':^{W}}\033[0m\n")
    time.sleep(1)


def _muerte_final(nombre):
    """Muerte personalizada de cada sobreviviente en el desenlace."""
    muertes = {
        "Ben": ("A Ben lo encontraron en el estudio, sentado frente al "
                "libro de contabilidad abierto. Tenía un bolígrafo rojo "
                "en la mano. Había escrito la misma palabra en cada "
                "página, cientos de veces, hasta que la tinta se acabó "
                "y siguió escribiendo con la presión del bolígrafo seco "
                "sobre el papel: 'SALDADO. SALDADO. SALDADO.'",
                "Sus ojos estaban abiertos. Su boca, congelada en una "
                "sonrisa que no era suya."),
        "Lisa": ("A Lisa la encontraron en la galería, de pie frente al "
                 "cuadro de los cinco visitantes. Tenía su libreta abierta "
                 "contra el pecho, apretada con ambas manos. La última "
                 "página estaba llena de una sola frase repetida con letra "
                 "cada vez más pequeña, cada vez más apretada, hasta "
                 "volverse ilegible: 'Yo soy la sexta. Yo soy la sexta. "
                 "Yo soy la sexta.'",
                 "No tenía heridas. Pero su expresión era la de alguien "
                 "que entendió algo que no debería haber entendido."),
        "Robert": ("A Robert lo encontraron en el lobby, sentado en el "
                   "sillón más alejado de la chimenea. El mismo donde se "
                   "sentó cuando llegó. Tenía la carta del padre en las "
                   "manos, doblada con cuidado. Pero la carta ya no decía "
                   "lo mismo. Alguien había reescrito cada línea durante "
                   "la noche, con la misma letra del padre, con las mismas "
                   "palabras, excepto el nombre del destinatario.",
                   "Ya no decía Robert. Decía Simón."),
        "Ana": ("A Ana la encontraron en la habitación de Simón, acostada "
                "en la cama que Simón nunca deshizo. Tenía el estuche de "
                "joyas abierto sobre el pecho. Las joyas habían vuelto. "
                "Todas. Intactas. Colocadas con cuidado sobre su cuerpo "
                "como adornos fúnebres: el collar sobre la garganta, "
                "los anillos en los dedos, los pendientes en las orejas.",
                "Alguien la vistió de muerta mientras dormía. "
                "O mientras no podía moverse."),
        "Lucas": ("A Lucas lo encontraron en la sala de vigilancia, "
                  "sentado en la silla frente a las mirillas. Tenía el "
                  "relicario abierto en la mano. Pero la inscripción "
                  "había cambiado. Ya no decía 'Para Lucas. Siempre.' "
                  "Decía 'Para Lucas. Por fin.'",
                  "En la mirilla central, el espejo seguía mostrando "
                  "la habitación del ala norte. La silla donde estuvo "
                  "Simón. Pero ahora había alguien sentado en ella. "
                  "Alguien que miraba directamente hacia la mirilla. "
                  "Directamente hacia Lucas. Sonriendo."),
    }
    if nombre in muertes:
        desc, detalle = muertes[nombre]
        glitch(intensidad=2)
        narrar(desc)
        time.sleep(0.5)
        acotacion(detalle)
        pausa()


# ══════════════════════════════════════════════════════════════
#  UTILIDADES DE CIERRE Y RESUMEN
# ══════════════════════════════════════════════════════════════

def _cierre_capitulo():
    G.fin_capitulo()
    abandonado = G.verificar_abandonos()
    if abandonado:
        _escena_abandono(abandonado)
        _evento_tension_asesino(abandonado)
    else:
        _atmosfera_entre_capitulos()
    G.mostrar_grupo()
    pausa()


def _atmosfera_entre_capitulos():
    """Detalles de época y tensión entre capítulos cuando nadie muere."""
    cap = G.capitulo
    if cap == 1:
        separador()
        narrar("Afuera, el sol de la tarde se esconde detrás de las colinas. "
               "La radio del lobby emite un chasquido y, por un instante, "
               "entre la estática, se escucha algo que podría ser música. "
               "Un vals. Lejano. Como si viniera de otra época o de otra "
               "habitación de la mansión.")
        narrar("Luego, silencio. Solo el crujir de la madera vieja "
               "y el tic-tac del reloj de péndulo del pasillo, que nadie "
               "recuerda haber escuchado antes.")
    elif cap == 2:
        separador()
        narrar("La luz de las lámparas de aceite proyecta sombras largas "
               "en las paredes. El olor a queroseno se mezcla con algo "
               "más antiguo: humedad, papel viejo, y debajo de todo, "
               "algo dulce y metálico que nadie quiere nombrar.")
        narrar("En algún lugar de la mansión, una puerta se cierra sola. "
               "El grupo se mira. Nadie dice nada. Pero todos contaron "
               "las personas en la habitación. Todos siguen siendo "
               "los mismos. ¿Verdad?")
    elif cap == 3:
        separador()
        narrar("La oscuridad de la noche se ha instalado por completo. "
               "Las ventanas son rectángulos negros que reflejan el "
               "interior de la mansión como espejos oscuros. Si miras "
               "demasiado tiempo, juras que los reflejos se mueven "
               "medio segundo después que tú.")
        narrar("El gramófono del lobby empieza a girar solo. La aguja "
               "baja sobre el disco. No hay música. Solo un susurro "
               "constante, rítmico, como una respiración amplificada "
               "por la bocina de latón.")
        acotacion("Nadie lo apaga. Nadie se atreve a acercarse.")
    elif cap == 4:
        separador()
        narrar("El reloj de péndulo del pasillo se detuvo. Marca las "
               "diez y cuarto. Pero afuera la oscuridad es la de la "
               "medianoche. El tiempo dentro de la mansión ya no "
               "coincide con el tiempo de afuera.")
        narrar("Alguien nota que las fotografías del tablero de corcho "
               "del estudio han cambiado de posición. La sexta foto, "
               "la de la cinta negra, ahora está en el centro. "
               "Y la cinta se ha despegado parcialmente.")
        narrar("Debajo de la cinta, apenas visible, hay un ojo. "
               "Pintado. Abierto. Mirando directamente hacia la puerta.")
        acotacion("Nadie recuerda haberla movido. Nadie quiere "
                  "preguntar quién lo hizo.")


def _evento_tension_asesino(abandonado):
    """Después de que alguien muere, la mansión reacciona."""
    n_muertos = sum(1 for p in G.personajes.values() if not p.presente)
    if n_muertos == 1:
        glitch(intensidad=2)
        narrar("Mientras el grupo intenta procesar lo que acaba de ver, "
               "las luces del lobby parpadean tres veces y se apagan. "
               "Oscuridad total. Alguien grita. Las luces vuelven.")
        narrar("Hay algo nuevo en la repisa de la chimenea: una fotografía "
               "que antes no estaba ahí. Es una foto del grupo. Tomada hoy. "
               "Desde algún lugar dentro de la mansión. Todos aparecen.")
        narrar(f"Sobre el rostro de {abandonado}, alguien dibujó una X "
               f"con algo que no es tinta. Es demasiado oscuro. Demasiado "
               f"espeso. Huele a hierro.")
        acotacion("Sangre. La X está dibujada con sangre.")
    elif n_muertos == 2:
        narrar("Un sonido recorre la mansión. No es una puerta. Es algo "
               "arrastrándose por el piso de arriba. Lento. Pesado. "
               "Húmedo. Como si alguien arrastrara algo que ya no puede "
               "moverse por sí mismo.")
        narrar("El sonido se detiene justo encima de donde está el grupo.")
        time.sleep(0.8)
        narrar("Silencio.")
        time.sleep(0.5)
        narrar("Luego, desde arriba, algo gotea a través de las tablas "
               "del techo. Una gota. Dos. Tres. Caen sobre la mesa del "
               "lobby. Son rojas.")
        acotacion("Nadie mira hacia arriba. Nadie quiere confirmar "
                  "lo que ya saben.")
    elif n_muertos == 3:
        narrar("En la mesa del lobby aparece una nota que no estaba antes. "
               "La tinta todavía está húmeda. No. No es tinta.")
        narrar("Dice: 'QUEDAN POCOS.' Las letras están escritas con un "
               "dedo. Se pueden ver las huellas dactilares en cada trazo. "
               "Debajo, más pequeño: 'Los estoy mirando ahora mismo.'")
        acotacion("El grupo se gira hacia las ventanas. Hacia las puertas. "
                  "Hacia las sombras. No hay nadie visible. "
                  "Pero la sensación de ser observado no se va.")
    else:
        narrar("Ya no hay sonidos extraños. Ya no hay notas. Ya no hay "
               "señales. El silencio de la mansión es absoluto, total, "
               "el tipo de silencio que solo existe cuando algo terrible "
               "ya terminó de pasar.")
        narrar("O cuando está a punto de pasar de nuevo.")
    pausa()
    # Efecto en los sobrevivientes
    for n in G.presentes():
        G.p(n).aislar(5)


def _escena_abandono(nombre):
    # Pantalla negra antes de la muerte
    pantalla_negra(2)
    beep()
    titulo_horror(f"{nombre.upper()}", "")
    time.sleep(1.5)
    textos = {
        "Ben": (
            "Ben llevaba rato callado. Demasiado callado. Tenía la mirada "
            "fija en un punto de la pared que nadie más podía ver. "
            "En algún momento se levantó del sillón y caminó hacia el "
            "pasillo del ala este sin decir nada. Sus pasos sonaban "
            "huecos en la madera. Nadie lo siguió. Nadie lo detuvo.",

            "Veinte minutos después, alguien fue a buscarlo.",

            "La puerta de la habitación estaba cerrada desde afuera. "
            "Tuvieron que forzarla. El olor los golpeó primero: cobre "
            "y algo más dulce, más enfermizo. Ben estaba en el suelo, "
            "boca arriba, con los ojos abiertos mirando al techo. "
            "Tenía la boca entreabierta como si hubiera intentado gritar "
            "y algo se lo hubiera impedido. Su corbata —la misma que "
            "llevaba aflojada desde que llegó— estaba enrollada alrededor "
            "de su cuello con una precisión quirúrgica. Tres vueltas. "
            "Apretada hasta que los vasos de los ojos reventaron.",

            "El libro de cuentas estaba abierto sobre su pecho. En la "
            "página donde aparecía su nombre, alguien había escrito con "
            "el mismo bolígrafo rojo de las entradas: 'SALDADO.' "
            "Ben murió con su deuda encima. Literalmente."
        ),
        "Lisa": (
            "Lisa dijo que necesitaba verificar algo en la galería. "
            "'Vuelvo en cinco minutos,' dijo sin mirar a nadie. "
            "Su voz sonaba distante, como si ya estuviera en otro lugar. "
            "Nadie la acompañó. Nadie pensó que debía hacerlo.",

            "No volvió en cinco minutos. Ni en diez. Ni en veinte.",

            "La encontraron en el taller, sentada contra la pared junto "
            "a la puerta trasera. Al principio parecía dormida. Luego "
            "vieron la mancha. Se extendía desde debajo de ella como una "
            "sombra oscura que crecía despacio sobre la madera del suelo. "
            "Tenía las manos sobre el abdomen, presionando algo que ya "
            "no podía contener. Su libreta estaba abierta en el suelo "
            "a medio metro. La última línea escrita con letra temblorosa "
            "decía: 'Hay alguien detrás de m' — la frase se cortaba en "
            "un trazo que bajaba hasta el borde de la página, como si "
            "la mano hubiera sido arrancada del papel.",

            "La carpeta con la evidencia no estaba por ningún lado. "
            "Pero en la pared, sobre la cabeza de Lisa, alguien había "
            "escrito con los dedos mojados en rojo: 'SILENCIO.'"
        ),
        "Robert": (
            "Robert se había ido quedando cada vez más quieto, más rígido, "
            "como si cada hora en la mansión le costara algo que no podía "
            "nombrar. En algún momento subió al segundo piso solo. "
            "Sus pasos en la escalera sonaron como los de un hombre "
            "que camina hacia algo que ya decidió enfrentar.",

            "Nadie lo escuchó gritar. Eso fue lo peor.",

            "Lo encontraron en la habitación de Simón, sentado en la "
            "silla frente al escritorio, con la espalda perfectamente "
            "recta —como siempre— y la cabeza inclinada hacia adelante "
            "como si estuviera leyendo algo. Pero no estaba leyendo. "
            "Sus ojos estaban abiertos y vacíos. Tenía las manos sobre "
            "la mesa, con las palmas hacia arriba, y en cada palma "
            "alguien había colocado un objeto: en la izquierda, la "
            "fotografía de 'Padre e hijo. 1987.' En la derecha, la carta "
            "sellada con lacre que Robert había venido a buscar. Abierta. "
            "Leída. Manchada de sangre en las esquinas.",

            "No había heridas visibles. Pero la expresión de su rostro "
            "era la de alguien que vio algo en los últimos segundos de "
            "su vida que fue peor que morir. Quien lo mató quiso que "
            "Robert supiera exactamente por qué."
        ),
        "Ana": (
            "Ana empezó a temblar. No de frío. Era el tipo de temblor "
            "que viene del lugar donde se guardan las cosas que no se "
            "dicen. 'No puedo seguir aquí,' susurró con la voz quebrada. "
            "Se alejó del grupo hacia la escalera. La escucharon subir. "
            "Escucharon una puerta cerrarse.",

            "Después, un golpe. Húmedo. Final.",

            "La encontraron al pie de la escalera del ala norte. El "
            "impacto la había dejado en una posición que el cuerpo humano "
            "no debería poder adoptar. Un brazo doblado en un ángulo "
            "imposible. La cabeza girada demasiado. Un charco oscuro "
            "se expandía lentamente bajo ella, buscando las grietas "
            "entre las tablas del suelo. La barandilla del segundo piso "
            "estaba rota en el punto exacto donde alguien podría "
            "apoyarse. Rota desde atrás. Empujada.",

            "El estuche de joyas estaba en el suelo junto a ella. "
            "Abierto. Vacío. Y dentro, donde deberían estar las joyas, "
            "había un espejo pequeño. Para que lo último que Ana viera "
            "al abrirlo fuera su propio rostro."
        ),
        "Lucas": (
            "Lucas dejó de hablar primero. Luego dejó de mirar a los "
            "demás. Se fue replegando hacia los rincones de cada "
            "habitación, haciéndose cada vez más pequeño, más invisible. "
            "La última vez que alguien lo vio estaba de pie frente al "
            "cuadro del niño con el relicario. Inmóvil. Cuando se "
            "giraron de nuevo, Lucas ya no estaba.",

            "Su maletín seguía en el suelo del lobby. Abierto. "
            "Con un rastro de gotas oscuras que salía de él hacia "
            "la puerta del sótano.",

            "Lo encontraron al pie de la escalera que baja a la sala "
            "de vigilancia. Estaba de espaldas, con los brazos extendidos "
            "como si hubiera intentado agarrarse de algo al caer. "
            "Tenía marcas en las muñecas —rojas, profundas, del tipo "
            "que deja una cuerda fina cuando alguien forcejea— aunque "
            "no había cuerda a la vista. En la mirilla más cercana, "
            "el reflejo del espejo mostraba el pasillo por donde Lucas "
            "había bajado. Y en el polvo del suelo, dos juegos de "
            "huellas: las de Lucas, y otras más grandes, más pesadas, "
            "que caminaban justo detrás de las suyas. Tan cerca que podría "
            "haberlo tocado. Tan cerca que probablemente lo hizo.",

            "El relicario estaba en su mano cerrada. Apretado con la "
            "fuerza de alguien que se aferra a lo último que le importa. "
            "La inscripción 'Para Lucas. Siempre.' brillaba bajo la luz "
            "de la vela caída. Lucas murió agarrado a un nombre que ni "
            "siquiera era el suyo."
        ),
    }
    partes = textos.get(nombre, (
        f"{nombre} se separó del grupo.",
        "Nadie lo acompañó.",
        f"Lo encontraron después. El cuerpo estaba frío.",
        "El aislamiento lo convirtió en presa fácil."
    ))
    narrar(partes[0])
    pausa()
    time.sleep(0.5)
    # Momento de tensión antes del descubrimiento
    pantalla_negra(1.5)
    limpiar()
    print()
    escribir_lento(partes[1])
    time.sleep(1.5)
    beep()
    glitch(intensidad=3)
    time.sleep(0.3)
    narrar(partes[2])
    time.sleep(1)
    texto_sangre(partes[3])
    separador()
    n_vivos = len(G.presentes())
    if n_vivos <= 2:
        escribir_lento(f"{nombre} está muerto.")
        time.sleep(0.5)
        escribir_lento("Y ustedes son los siguientes.")
    else:
        escribir_lento(f"{nombre} está muerto.")
        time.sleep(0.5)
        acotacion("El aislamiento lo separó del grupo. Y quien acecha "
                  "en esta mansión caza a los que se quedan solos. "
                  "Con paciencia. Con método. Con placer.")
    pausa()


def resumen_final():
    titulo("R E S U M E N   D E   P A R T I D A")
    vivos = G.presentes()
    objetos = sum(1 for p in G.personajes.values() if p.objeto_encontrado)

    print(f"  Sobrevivientes     : {len(vivos)}/5")
    for n in vivos:
        obj = "✓" if G.p(n).objeto_encontrado else "✗"
        print(f"    {n:8s}  {obj} {G.p(n).objeto}")
    ausentes = [n for n, p in G.personajes.items() if not p.presente]
    if ausentes:
        print(f"\n  No sobrevivieron   : {', '.join(ausentes)}")
    print(f"\n  Simón rescatado    : {'Sí' if G.simon_encontrado else 'No'}")
    print(f"  Pistas descubiertas: {len(G.pistas)}")
    print(f"  Decisiones tomadas : {len(G.decisiones)}")

    separador()
    print("  Pistas clave:")
    claves = [
        ("simon_vivo",         "Carta inconclusa: 'No estoy muerto'"),
        ("grabacion_simon",    "Grabación de Simón: 'No son todos enemigos'"),
        ("codigo_4729",        "Código 4-7-2-9 del cuadro abstracto"),
        ("mancha_sangre",      "Mancha de sangre en la galería"),
        ("foto_cinta_negra",   "Sexta fotografía con rostro cubierto"),
        ("ubicacion_simon",    "Mapa con ubicación exacta de Simón"),
        ("grabacion_amenaza",  "Grabación: 'No voy a callar más'"),
        ("sobre_testigo",      "Sobre marcado TESTIGO"),
        ("segunda_copia",      "Simón tenía una segunda copia de seguridad"),
    ]
    encontradas = 0
    for k, desc in claves:
        if G.tiene(k):
            print(f"    ✓ {desc}")
            encontradas += 1
        else:
            print(f"    ✗ {desc}")
    print(f"\n  {encontradas}/{len(claves)} pistas clave encontradas")
    separador()


# ══════════════════════════════════════════════════════════════
#  MAIN
# ══════════════════════════════════════════════════════════════
#  POST-CRÉDITOS — El último escalofrio
# ══════════════════════════════════════════════════════════════

def post_creditos():
    """El mensaje final que el jugador no espera."""
    resumen_final()
    print(f"\n{'Gracias por jugar La Mansión de Simón.':^{W}}")
    print(f"\033[2m{'Europa, 1943':^{W}}\033[0m\n")
    pausa()

    # El jugador cree que terminó.
    pantalla_negra(5)

    # El gramófono
    time.sleep(1)
    sys.stdout.write("  ")
    for c in "...":
        sys.stdout.write(c); sys.stdout.flush()
        time.sleep(0.8)
    print()
    time.sleep(2)

    # Un sonido
    sys.stdout.write("  ")
    for c in "(Se escucha un crujido. Como una puerta que se abre.)":
        sys.stdout.write(c); sys.stdout.flush()
        time.sleep(0.04)
    print()
    time.sleep(3)

    limpiar()
    time.sleep(2)

    # Texto lento, como si alguien escribiera en una máquina de escribir
    def maquina(texto, pausa_entre=0.06):
        sys.stdout.write("  ")
        for c in texto:
            sys.stdout.write(c); sys.stdout.flush()
            if c in '.?!':
                time.sleep(0.5)
            elif c == ',':
                time.sleep(0.2)
            elif c == ' ':
                time.sleep(0.1)
            else:
                time.sleep(pausa_entre)
        print()

    maquina("¿Sigues ahí?")
    time.sleep(3)

    glitch(intensidad=1)
    time.sleep(1.5)

    maquina("Bien.", 0.15)
    time.sleep(3)

    limpiar()
    time.sleep(2)

    maquina("Quiero contarte algo.")
    time.sleep(2)

    maquina("Algo que los personajes no pudieron decirte.")
    time.sleep(2)

    maquina("Porque estaban muertos cuando lo entendieron.")
    time.sleep(3)

    limpiar()
    time.sleep(2)

    # La revelación
    escribir_miedo("Cada decisión que tomaste esta noche fue observada.")
    time.sleep(1.5)

    escribir_miedo("Cada puerta que abriste, alguien la dejó abierta "
                   "para ti.")
    time.sleep(1.5)

    escribir_miedo("Cada pista que encontraste fue colocada donde "
                   "sabía que mirarías.")
    time.sleep(1.5)

    escribir_miedo("Cada persona que murió, murió porque tú elegiste "
                   "no hablarle.")
    time.sleep(2)

    limpiar()
    time.sleep(2)

    escribir_miedo("Tú decidiste quién vivía y quién moría.")
    time.sleep(1.5)

    escribir_miedo("Y lo hiciste sin dudar.")
    time.sleep(1.5)

    escribir_miedo("Como si fuera un juego.")
    time.sleep(2)

    limpiar()
    time.sleep(2.5)

    # El golpe
    glitch(intensidad=4)
    time.sleep(0.5)
    beep()

    limpiar()
    time.sleep(1.5)

    maquina("¿Recuerdas la sexta fotografía?")
    time.sleep(2)

    maquina("La del tablero de corcho en el estudio.")
    time.sleep(1.5)

    maquina("La que tenía el rostro cubierto con cinta negra.")
    time.sleep(2.5)

    limpiar()
    time.sleep(2)

    maquina("¿Quieres saber quién es?")
    time.sleep(3)

    limpiar()
    time.sleep(2)

    glitch(intensidad=3)
    time.sleep(0.3)

    # El momento — ERES TÚ en grande, rojo, que ocupe la pantalla
    limpiar()
    time.sleep(0.5)
    beep()
    eres_tu = [
        "  ███████ ██████  ███████ ███████     ████████ ██    ██ ",
        "  ██      ██   ██ ██      ██             ██    ██    ██ ",
        "  █████   ██████  █████   ███████        ██    ██    ██ ",
        "  ██      ██   ██ ██           ██        ██    ██    ██ ",
        "  ███████ ██   ██ ███████ ███████        ██     ██████  ",
    ]
    print("\n" * 2)
    for linea in eres_tu:
        print(f"\033[31m\033[1m{linea}\033[0m")
        time.sleep(0.15)
    print("\n")
    beep()
    time.sleep(5)

    limpiar()
    time.sleep(2)

    # La explicación que hiela
    escribir_miedo("Simón te pintó antes de que llegaras.")
    time.sleep(2)

    escribir_miedo("No a Ben. No a Lisa. No a Robert, ni a Ana, "
                   "ni a Lucas.")
    time.sleep(1.5)

    escribir_miedo("A ti.")
    time.sleep(2)

    escribir_miedo("El que observa. El que explora. El que decide "
                   "quién sobrevive.")
    time.sleep(2)

    limpiar()
    time.sleep(2)

    escribir_miedo("Tú eras la sexta persona en la mansión.")
    time.sleep(1.5)

    escribir_miedo("Tú estuviste ahí toda la noche.")
    time.sleep(1.5)

    escribir_miedo("Abriendo puertas. Leyendo cartas. Eligiendo "
                   "a quién ignorar.")
    time.sleep(2)

    limpiar()
    time.sleep(2)

    glitch(intensidad=5)
    beep()
    time.sleep(0.5)

    limpiar()
    time.sleep(2)

    escribir_miedo("Y ahora que terminó...")
    time.sleep(2)

    escribir_miedo("¿Estás seguro de que saliste?")
    time.sleep(3)

    limpiar()
    time.sleep(3)

    # MIRA DETRÁS DE TI — gigante, rojo, impacto total
    limpiar()
    time.sleep(1)
    glitch(intensidad=4)
    time.sleep(0.3)
    beep()

    mira = [
        " ███    ███ ██ ██████   █████  ",
        " ████  ████ ██ ██   ██ ██   ██ ",
        " ██ ████ ██ ██ ██████  ███████ ",
        " ██  ██  ██ ██ ██   ██ ██   ██ ",
        " ██      ██ ██ ██   ██ ██   ██ ",
    ]
    detras = [
        " ██████  ███████ ████████ ██████   █████  ███████ ",
        " ██   ██ ██         ██    ██   ██ ██   ██ ██      ",
        " ██   ██ █████      ██    ██████  ███████ ███████ ",
        " ██   ██ ██         ██    ██   ██ ██   ██      ██ ",
        " ██████  ███████    ██    ██   ██ ██   ██ ███████ ",
    ]
    de_ti = [
        " ██████  ███████     ████████ ██ ",
        " ██   ██ ██             ██    ██ ",
        " ██   ██ █████          ██    ██ ",
        " ██   ██ ██             ██    ██ ",
        " ██████  ███████        ██    ██ ",
    ]

    print("\n")
    for linea in mira:
        print(f"\033[37m\033[1m{linea}\033[0m")
        time.sleep(0.1)
    print()
    time.sleep(0.5)
    for linea in detras:
        print(f"\033[31m\033[1m{linea}\033[0m")
        time.sleep(0.1)
    print()
    time.sleep(0.5)
    for linea in de_ti:
        print(f"\033[31m\033[1m{linea}\033[0m")
        time.sleep(0.1)
    print()
    beep()
    time.sleep(6)

    limpiar()
    time.sleep(2)

    glitch(intensidad=8)
    beep()
    time.sleep(0.3)

    limpiar()
    time.sleep(3)

    # Silencio largo. Luego el cierre.
    print(f"\n\n\033[2m{'La mansión sigue abierta.':^{W}}\033[0m")
    time.sleep(2.5)
    print(f"\033[2m{'La puerta nunca se cerró.':^{W}}\033[0m")
    time.sleep(2.5)

    # La última línea — en rojo
    print()
    time.sleep(1)
    tu_adentro = "Y  T Ú  S I G U E S  A D E N T R O ."
    print(f"\033[31m\033[1m{tu_adentro:^{W}}\033[0m")
    beep()
    time.sleep(5)
    print()


# ══════════════════════════════════════════════════════════════

def main():
    prologo()
    capitulo_1()
    capitulo_2()
    capitulo_3()
    capitulo_4()
    capitulo_5()
    determinar_final()
    post_creditos()

if __name__ == "__main__":
    main()
