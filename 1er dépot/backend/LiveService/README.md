# Live Service Microservice

## Overview
The Live Service microservice is designed to manage live sessions and provide real-time chat functionality using WebSockets. It integrates with the api.video service for video management and allows users to interact through a chat interface during live sessions.

## Project Structure
The project is organized into several folders, each serving a specific purpose:

- **Controllers**: Contains the controllers that handle HTTP requests and WebSocket connections.
- **Data**: Contains the database context for managing data access.
- **Dtos**: Contains Data Transfer Objects for transferring data between layers.
- **Enum**: Contains enumerations for various states and roles within the application.
- **Interfaces**: Contains interfaces that define the contracts for services and repositories.
- **Mappers**: Contains classes for mapping between models and DTOs.
- **Middleware**: Contains middleware for handling authentication and security headers.
- **Models**: Contains the data models for live sessions and chat messages.
- **Repositories**: Contains classes for data access related to live sessions.
- **Services**: Contains business logic for live sessions and chat functionality.
- **WebSockets**: Contains classes for managing WebSocket connections and chat processing.

## Setup Instructions
1. **Clone the Repository**
   ```bash
   git clone <repository-url>
   cd LiveService
   ```

2. **Install Dependencies**
   Ensure you have the .NET SDK installed, then run:
   ```bash
   dotnet restore
   ```

3. **Configure the Application**
   Update the `appsettings.json` file with your database connection string and api.video API key.

4. **Run the Application**
   To start the application, use:
   ```bash
   dotnet run
   ```

5. **Access the API**
   The API will be available at `http://localhost:5000`. You can use tools like Postman or curl to interact with the endpoints.

## Usage
- **Live Sessions**: Use the LiveController to create, update, and retrieve live sessions.
- **Real-Time Chat**: Connect to the WebSocket endpoint to send and receive chat messages in real-time.

## Contributing
Contributions are welcome! Please submit a pull request or open an issue for any enhancements or bug fixes.

## License
This project is licensed under the MIT License. See the LICENSE file for more details.