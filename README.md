# Cukraszda gRPC Server and Client Application

## Overview
This project is a gRPC-based application that implements a server-client architecture for managing confectionery data. The application includes the following features:

- **CRUD operations** for cakes and desserts.
- Token-based authentication to secure Create, Update, and Delete operations.
- A command-line client that interacts with the gRPC server.

## Features

### 1. Authentication
The client authenticates by providing a username and password. Upon successful authentication, a token is issued, which is required for further operations.

### 2. gRPC Endpoints
The following endpoints are implemented in the gRPC server:

- **GetDijazottTortak**: Retrieves a list of awarded cakes sorted alphabetically.
- **GetAtlagArTipusonkent**: Returns the average price of desserts by type and unit.
- **GetLaktozmentesPitekEsTortaszeletek**: Retrieves lactose-free pies and cake slices by type.
- **CreateSutemeny**: Adds a new dessert.
- **UpdateSutemeny**: Updates an existing dessert.
- **DeleteSutemeny**: Deletes a dessert by ID.
- **GetSutemenyById**: Retrieves a dessert by its ID.

## Prerequisites

- [.NET 6.0 SDK or later](https://dotnet.microsoft.com/download)
- MySQL database
- gRPC tools for .proto file compilation

## Setup Instructions

### 1. Clone the repository
```bash
git clone <repository-url>
cd Cukraszda_Server
```

### 2. Configure the database connection
Edit the `appsettings.json` file in the server project to set the correct connection string for your MySQL database.

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=cukraszda;User=root;Password=yourpassword;"
  }
}
```

### 3. Run the gRPC server
```bash
cd Cukraszda_Server
 dotnet run
```

The server will start and listen on `https://localhost:7068`.

### 4. Run the client
```bash
cd Cukraszda_Client
 dotnet run
```

Follow the on-screen instructions to interact with the server.

## Usage

### Example Commands

1. **Authenticate**: Enter your username and password when prompted.
2. **Get awarded cakes**:
   ```
   Select option 1 from the menu.
   ```
3. **Add a new dessert**:
   ```
   Select option 4 from the menu and provide the required details.
   ```

## Project Structure

```
Cukraszda_Server/
├── Protos/                  # .proto files for gRPC definitions
├── Services/                # gRPC service implementations
├── appsettings.json         # Configuration file
Cukraszda_Client/
├── Program.cs               # Main client application
└── Protos/                  # Generated client-side gRPC code
```

## gRPC Protos
The following `.proto` files define the gRPC services and messages:

### `cukraszda.proto`
Defines the `CukraszdaService` with the CRUD operations and queries.

### `auth.proto`
Defines the `AuthService` for user authentication.

## License
This project is licensed under the MIT License.

## Acknowledgments
Special thanks to the contributors and the open-source community for their support in building this project.
