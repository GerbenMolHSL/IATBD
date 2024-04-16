namespace IATWeb;

// Animal types with int as value
public enum AnimalTypes
{
    Hond = 1,
    Kat = 2,
    Konijn = 3,
    Vogel = 4,
    Vis = 5,
    Reptiel = 6,
    Paard = 7,
    Knaagdier = 8,
    Overig = 9
}

// Rating with int as value till 10
public enum Rating
{
    Een = 1,
    Twee = 2,
    Drie = 3,
    Vier = 4,
    Vijf = 5,
    Zes = 6,
    Zeven = 7,
    Acht = 8,
    Negen = 9,
    Tien = 10
}

// Request status with int as value
public enum RequestStatus
{
    InAfwachting = 0,
    Goedgekeurd = 1,
    Afgerond = 2,
    ActionRequired = 3
}