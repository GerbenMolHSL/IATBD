DB Explanation

Animals:

| Field | Type          | Description                                                                                                                                                             |
|-------|---------------|-------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| id    | int           | Primary Key                                                                                                                                                             |
| name  | nvarchar(255) | Name of the pet                                                                                                                                                         |
| type  | smallint      | Type of the pet<br> Types are <br> Hond = 1 <br> Kat = 2 <br> Konijn = 3 <br> Vogel = 4 <br> Vis = 5 <br> Reptiel = 6 <br> Paard = 7 <br> Knaagdier = 8 <br> Overig = 9 |
| owner | nvarchar(255) | FK to Users                                                                                                                                                             |
| payment | smallmoney    | Payment per hour                                                                                                                                                        |

Users:

| Field | Type          | Description                                                                                                                                                             |
|-------|---------------|-------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| id    | nvarchar(255) | Primary Key                                                                                                                                                             |
| name  | nvarchar(255) | Name of the user                                                                                                                                                        |
| hsdPsswrd | nvarchar(max) | Hashed password of the user                                                                                                                                            |

Requests:

| Field | Type          | Description                                                                                                                 |
|-------|---------------|-----------------------------------------------------------------------------------------------------------------------------|
| id    | int           | Primary Key                                                                                                                 |
| owner | nvarchar(255) | FK to Users                                                                                                                 |
| pet   | int           | FK to Animals                                                                                                               |
| starttime | datetime | Start time of the request                                                                                                   |
| endtime | datetime | End time of the request                                                                                                     |
| status | smallint | Status of the request<br> Statuses are <br> InAfwachting = 0 <br> Goedgekeurd = 1 <br> Afgerond = 2 <br> ActionRequired = 3 |

Sessions

| Field | Type          | Description                                                                                                             |
|-------|---------------|-------------------------------------------------------------------------------------------------------------------------|
| id    | int           | Primary Key                                                                                                             |
| session | uniqueidentifier | Session ID                                                                                                             |
| user | nvarchar(255) | FK to Users                                                                                                             |