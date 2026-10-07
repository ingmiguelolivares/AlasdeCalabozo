using System;
using System.Collections.Generic;
using UnityEngine;

public static class BirdCardProgress
{
    public const int TotalCollectionCards = 20;

    public static readonly string[] CardNames =
    {
        "AGUILA PESCADORA", "ALCARAVAN", "BUHO LISTADO", "CARPINTERO AHUMADO",
        "CHIRLOBIRLO", "CHORLITO GRITON", "CHULO", "COPETON", "CORMORAN",
        "FOCHA AMERICANA", "GARCITA RAYADA", "GARZA REAL", "GARZA SILBADORA",
        "GAVILAN MAROMERO", "GUACO", "MIRLA PATINARANJA", "PATO PICO AZUL",
        "PERIQUITO DE ANTEOJOS", "SINSONTE", "TINGUA PICO VERDE"
    };

    static readonly Dictionary<string, BirdCardReward> RewardsByName = new Dictionary<string, BirdCardReward>(StringComparer.OrdinalIgnoreCase);

    static BirdCardProgress()
    {
        RegisterDefaults();
    }

    public static void RegisterBirdCard(BirdCardReward reward)
    {
        if (reward == null || string.IsNullOrWhiteSpace(reward.commonName))
            return;

        RewardsByName[reward.commonName] = reward;
    }

    public static BirdCardReward RegisterBirdCard(string commonName)
    {
        BirdCardReward reward = GetReward(commonName);
        UnlockCard(reward.commonName);
        return reward;
    }

    public static void UnlockCard(string commonName)
    {
        if (string.IsNullOrWhiteSpace(commonName))
            return;

        PlayerPrefs.SetInt("carta_" + commonName, 1);
        PlayerPrefs.SetInt("cartasDesbloqueadas_" + commonName, 1);
        PlayerPrefs.Save();
        Debug.Log("[BirdCardProgress] Carta registrada: " + commonName);
    }

    public static int GetCollectedCardsCount()
    {
        int count = 0;
        foreach (string cardName in CardNames)
        {
            if (PlayerPrefs.GetInt("carta_" + cardName, 0) == 1 ||
                PlayerPrefs.GetInt("cartasDesbloqueadas_" + cardName, 0) == 1)
            {
                count++;
            }
        }

        return count;
    }

    public static bool HasCard(string commonName)
    {
        return PlayerPrefs.GetInt("carta_" + commonName, 0) == 1 ||
               PlayerPrefs.GetInt("cartasDesbloqueadas_" + commonName, 0) == 1;
    }

    public static BirdCardReward GetRewardForBattle(int battleIndex)
    {
        RegisterDefaults();
        int safeIndex = Mathf.Clamp(battleIndex, 0, CardNames.Length - 1);
        return GetReward(CardNames[safeIndex]);
    }

    public static BirdCardReward GetReward(string commonName)
    {
        RegisterDefaults();
        BirdCardReward reward;
        if (RewardsByName.TryGetValue(commonName, out reward))
            return reward;

        return new BirdCardReward
        {
            commonName = commonName,
            scientificName = "",
            habitat = "Humedales, bosques o espacios abiertos de la Sabana de Bogota.",
            curiousFact = "Esta carta aun no tiene ficha extendida.",
            worldLore = "Un fragmento de conocimiento para enfrentar a la Esfinge.",
            sphinxClue = "Las aves desbloqueadas pueden revelar pistas en el encuentro final."
        };
    }

    public static string GetSphinxHint()
    {
        RegisterDefaults();
        foreach (string cardName in CardNames)
        {
            if (HasCard(cardName))
                return GetReward(cardName).sphinxClue;
        }

        return "Recolecta cartas de aves para desbloquear pistas de la Esfinge.";
    }

    static void RegisterDefaults()
    {
        if (RewardsByName.Count > 0)
            return;

        Add("AGUILA PESCADORA", "Pandion haliaetus", "Humedales, rios y lagunas con peces.", "Puede lanzarse al agua para capturar peces con sus garras.", "Su precision inspira ataques calculados.", "Busca respuestas relacionadas con vision, agua y caza precisa.");
        Add("ALCARAVAN", "Vanellus chilensis", "Pastizales abiertos y zonas rurales.", "Defiende su territorio con llamados fuertes.", "Su vigilancia anuncia peligros en el mapa.", "La Esfinge respeta a quienes reconocen guardianes de campo abierto.");
        Add("BUHO LISTADO", "Asio clamator", "Bosques, bordes de humedal y areas arboladas.", "Caza de noche usando oido fino.", "Su sabiduria nocturna abre pistas ocultas.", "Las preguntas sobre noche, silencio o escucha pueden relacionarse con esta carta.");
        Add("CARPINTERO AHUMADO", "Leuconotopicus fumigatus", "Bosques andinos y arboles maduros.", "Golpea troncos para buscar insectos.", "Marca rutas secretas en madera antigua.", "Piensa en arboles, picos fuertes e insectos escondidos.");
        Add("CHIRLOBIRLO", "Sturnella magna", "Pastizales altos y sabanas.", "Su canto se escucha a larga distancia.", "Su voz guia al grupo entre calabozos.", "El canto y los pastizales pueden ser la clave.");
        Add("CHORLITO GRITON", "Charadrius vociferus", "Orillas, humedales y suelos abiertos.", "Distrae depredadores fingiendo heridas.", "Enseña a sobrevivir con estrategia.", "No todo poder es fuerza: a veces la respuesta es distraccion.");
        Add("CHULO", "Coragyps atratus", "Ciudades, campos y bordes de carretera.", "Limpia el ecosistema alimentandose de carrona.", "Transforma ruina en equilibrio.", "La Esfinge puede preguntar por reciclaje natural o carroña.");
        Add("COPETON", "Zonotrichia capensis", "Jardines, parques y zonas urbanas.", "Es uno de los cantos mas comunes de la ciudad.", "Recuerda que lo cotidiano tambien da poder.", "Observa aves urbanas y pequenas para responder.");
        Add("CORMORAN", "Phalacrocorax brasilianus", "Lagos y humedales.", "Bucea para pescar y seca sus alas al sol.", "Domina agua y aire entre batallas.", "Agua, pesca y alas extendidas son pistas importantes.");
        Add("FOCHA AMERICANA", "Fulica americana", "Humedales con vegetacion flotante.", "Nada entre juncos y plantas acuaticas.", "Conoce los caminos escondidos del pantano.", "La vegetacion de humedal puede esconder la respuesta.");
        Add("GARCITA RAYADA", "Butorides striata", "Orillas de rios, lagunas y canales.", "Es paciente al acechar peces pequenos.", "Premia la espera precisa antes del golpe.", "Paciencia y orillas de agua apuntan a esta ave.");
        Add("GARZA REAL", "Ardea alba", "Humedales, lagunas y rios.", "Su cuello largo le permite cazar con rapidez.", "Representa elegancia y alcance.", "Cuello largo, plumaje claro y humedal son pistas.");
        Add("GARZA SILBADORA", "Syrigma sibilatrix", "Sabanas humedas y pastizales.", "Tiene un llamado silbado caracteristico.", "Su silbido abre puertas antiguas.", "Escucha el detalle del sonido en la pregunta.");
        Add("GAVILAN MAROMERO", "Elanus leucurus", "Campos abiertos y bordes de humedal.", "Puede suspenderse en el aire antes de caer sobre su presa.", "Enseña control antes de arriesgar.", "Quedarse suspendido en el aire puede resolver la pista.");
        Add("GUACO", "Nycticorax nycticorax", "Humedales y arboles cerca del agua.", "Es mas activo al atardecer y de noche.", "Vigila los limites entre dia y noche.", "Atardecer, noche y humedal son senales.");
        Add("MIRLA PATINARANJA", "Turdus fuscater", "Parques, jardines y bosques andinos.", "Busca frutos e insectos en el suelo.", "Su paso naranja deja huellas de conocimiento.", "Fijate en patas naranjas, frutos o suelo.");
        Add("PATO PICO AZUL", "Oxyura jamaicensis", "Lagunas altoandinas.", "El macho destaca por su pico azul intenso.", "Su color revela secretos de agua profunda.", "Un pico azul en lagunas altas no es casual.");
        Add("PERIQUITO DE ANTEOJOS", "Forpus conspicillatus", "Bosques secos, bordes y areas cultivadas.", "Vive en grupos pequenos y ruidosos.", "La cooperacion vence acertijos complejos.", "Grupo, ruido y marca alrededor de los ojos son pistas.");
        Add("SINSONTE", "Mimus gilvus", "Matorrales, jardines y zonas secas.", "Imita cantos de otras aves.", "Puede confundir o revelar verdades.", "Si la pregunta habla de imitacion, recuerda esta carta.");
        Add("TINGUA PICO VERDE", "Porphyriops melanops", "Humedales con vegetacion densa.", "Se mueve entre plantas acuaticas con sigilo.", "Guarda claves del humedal profundo.", "Pico verde, sigilo y humedal denso guian la respuesta.");
    }

    static void Add(string commonName, string scientificName, string habitat, string curiousFact, string worldLore, string sphinxClue)
    {
        RewardsByName[commonName] = new BirdCardReward
        {
            commonName = commonName,
            scientificName = scientificName,
            habitat = habitat,
            curiousFact = curiousFact,
            worldLore = worldLore,
            sphinxClue = sphinxClue
        };
    }
}

[Serializable]
public class BirdCardReward
{
    public string commonName;
    public string scientificName;
    [TextArea(2, 3)] public string habitat;
    [TextArea(2, 3)] public string curiousFact;
    [TextArea(2, 3)] public string worldLore;
    [TextArea(2, 3)] public string sphinxClue;

    public string ToRewardText()
    {
        return commonName + "\n" +
               scientificName + "\n" +
               "Habitat: " + habitat + "\n" +
               "Dato: " + curiousFact + "\n" +
               "Mundo: " + worldLore + "\n" +
               "Pista Esfinge: " + sphinxClue;
    }
}
