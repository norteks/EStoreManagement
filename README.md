# OnlineShopAPI – Programmēšana II projekts

## 📌 Projekta apraksts
Šis projekts ir izstrādāts kursa **Programmēšana II** ietvaros. Projekta mērķis ir izveidot REST API, izmantojot **ASP.NET Core**, **Entity Framework**, **Swagger** un **JWT autentifikāciju**.

API nodrošina vienkāršu Eveikala funkcionalitāti – preču, kategoriju un pasūtījumu pārvaldību, kā arī lietotāju autentifikāciju.

---

## 🛠️ Izmantotās tehnoloģijas
- ASP.NET Core Web API  
- Entity Framework Core  
- SQLite datubāze  
- Swagger (Swashbuckle.AspNetCore)  
- JWT (JSON Web Token) autentifikācija  

---

## 🗄️ Datu bāzes struktūra
Projektā tiek izmantota relāciju datubāze ar **4 tabulām**:

| Tabula     | Apraksts |
|------------|----------|
| Users      | Lietotāji (e-pasts) |
| Categories | Preču kategorijas |
| Products   | Preces ar cenu un kategoriju |
| Orders     | Pasūtījumi, kas piesaistīti lietotājiem |

Tabulas ir savienotas ar **ārējām atslēgām (Foreign Keys)**, izmantojot Entity Framework.

---

## 🌐 API Endpointi

### 🔹 Products
- `GET /api/products` – atgriež visu preču sarakstu  
- `POST /api/products` – pievieno jaunu preci  

### 🔹 Categories
- `GET /api/categories` – atgriež visas kategorijas  
- `POST /api/categories` – pievieno jaunu kategoriju  

### 🔹 Orders (aizsargāts ar JWT)
- `GET /api/orders` – atgriež visus pasūtījumus (pieejams tikai ar tokenu)

### 🔹 Authentication
- `POST /api/auth/login` – izveido JWT tokenu lietotājam

---

## 🔐 JWT Autentifikācija
JWT (JSON Web Token) tiek izmantots, lai aizsargātu API endpointus.

Tokena darbības princips:
1. Lietotājs piesakās ar `/api/auth/login`
2. API atgriež JWT tokenu
3. Tokenu pievieno pieprasījumam kā `Authorization: Bearer {token}`
4. Aizsargātie endpointi pārbauda tokenu

---

## 📘 Swagger
Swagger ir integrēts projektā un pieejams pēc API palaišanas:


Swagger piedāvā:
- visu endpointu dokumentāciju
- datu modeļu (schemas) apskati
- iespēju testēt GET un POST pieprasījumus
- JWT autorizāciju caur **Authorize** pogu

---

## ▶️ Kā palaist projektu
1. Klonē repozitoriju:
2. Atver projektu Visual Studio
3. Palaid projektu (`Run`)
4. Atver Swagger UI pārlūkā

---

## 🎥 Prezentācija
Projekta noslēgumā tika sagatavota video prezentācija, kurā:
- parādīta API darbība
- demonstrēta datubāze un Swagger
- izskaidrota JWT autentifikācija
- aprakstīta koda struktūra un izmantotās tehnoloģijas

👉 **Video saite:** (pievienot šeit)

---

## 👥 Komanda
- Komandas vadītājs: _(Kristers, Zujevs)_

---

## ✅ Secinājumi
Šis projekts palīdzēja apgūt:
- REST API pamatus
- darbu ar Entity Framework
- relāciju datubāžu modelēšanu
- Swagger izmantošanu dokumentēšanai
- JWT autentifikācijas ieviešanu
