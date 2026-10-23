# Password Manager

A simple desktop password manager built with **C#**, **Windows Forms** and **ADO.NET** (SQL Server). Log in with a master password and store website credentials in encrypted form.

## Features

- Master password login (created on first run, 3 attempts allowed)
- Add, edit, delete website credentials
- Live search by website or username
- Passwords encrypted with AES-256 before being saved to the database
- Show/hide password toggle

## How the security works

| Item | How it is protected |
|---|---|
| Master password | Never stored. Only a salted PBKDF2 hash (SHA-256, 100,000 iterations) is saved. |
| Website passwords | Encrypted with AES-256. The key is derived from the master password and is never stored. |
| Encryption IV | A new random IV is generated for every entry. |
| Database queries | All queries are parameterized to prevent SQL injection. |

> If the master password is forgotten, the stored passwords cannot be recovered.

## Tech stack

- C# / .NET 10
- Windows Forms
- ADO.NET with `Microsoft.Data.SqlClient`
- SQL Server Express

## Setup

1. Install Visual Studio with the **.NET desktop development** workload, and SQL Server Express.
2. Run `database.sql` to create the `PasswordManagerDB` database and its tables.
3. In `DatabaseHelper.cs`, set the connection string for your server, for example:
   ```csharp
   Server=.\SQLEXPRESS;Database=PasswordManagerDB;Trusted_Connection=True;TrustServerCertificate=True;
   ```
4. Open `PasswordManager.slnx` and press **F5**.

## Project structure

| File | Purpose |
|---|---|
| `Program.cs` | App entry point |
| `LoginForm.cs` | Master password creation and login |
| `MainForm.cs` | Entry list, search, add, update, delete |
| `DatabaseHelper.cs` | All ADO.NET database code |
| `CryptoHelper.cs` | Password hashing and AES encryption |

## Possible improvements

- Copy-to-clipboard with auto-clear
- Random password generator
- Change master password (re-encrypt all entries)
- Auto-lock after inactivity
- Switch AES-CBC to AES-GCM

## Disclaimer

This is a learning project. For real-world use of sensitive passwords, use an established, audited password manager.
