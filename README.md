# Padel in Dubai Backend

A robust backend system for managing padel court bookings, events, and integrations with external services.

## 🚀 Features

- Event Management System
- Integration with Altegio Booking System
- Telegram Bot Integration for notifications and user interactions
- RESTful API with Swagger Documentation
- PostgreSQL Database
- Background Services for Automated Tasks

## 🛠 Technology Stack

- .NET 9.0
- Entity Framework Core with PostgreSQL
- Telegram Bot API
- Swagger/OpenAPI
- Newtonsoft.Json
- Systemd for service management

## 📋 Prerequisites

- .NET 9.0 SDK
- PostgreSQL Database
- Telegram Bot Token
- Altegio API Credentials
- Linux server with systemd (for production deployment)

## 🔧 Installation

1. Clone the repository:
```bash
git clone [repository-url]
cd PadelInDubaiBE
```

2. Install dependencies:
```bash
dotnet restore
```

3. Build the application:
```bash
dotnet build -c Release
```

4. Publish the application:
```bash
dotnet publish -c Release
```

## ⚙️ Configuration

### Environment Variables
The application uses environment variables for sensitive configuration. These are managed through systemd service file:

```ini
[Unit]
Description=PadelInDubai
After=network.target

[Service]
User=usr
WorkingDirectory=/usr/padel-in-dubai/PadelInDubaiBE/Release
ExecStart=/usr/dotnet/dotnet PadelInDubai.dll
Restart=always
RestartSec=10
SyslogIdentifier=PadelInDubaiApi
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment="PD_connection="
Environment="PD_Tg_bot_token="
Environment="PD_TgChatId="
Environment="PD_bearer="

[Install]
WantedBy=multi-user.target
```

### Application Settings
The `appsettings.json` file contains non-sensitive configuration:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "Kestrel": {
    "Endpoints": {
      "Http": {
        "Url": "http://0.0.0.0:5152"
      }
    }
  }
}
```

## 📁 Project Structure

```
PadelInDubai/
├── Controllers/         # API endpoints
├── Services/           # Business logic
├── DAL/               # Data Access Layer
├── Models/            # Data models
├── Migrations/        # Database migrations
├── BackgroundServices/# Background tasks
├── HostedServices/    # Long-running services
├── Extensions/        # Extension methods
├── Middlewares/       # Custom middleware
├── Mappings/          # Object mapping profiles
├── Attributes/        # Custom attributes
└── Content/           # Static content
```

## 🔌 API Endpoints

### Events
- `GET /api/events` - Get all events
- `POST /api/events` - Create new event
- `GET /api/events/{id}` - Get event by ID
- `PUT /api/events/{id}` - Update event
- `DELETE /api/events/{id}` - Delete event

### Altegio Integration
- `POST /api/altegio/webhook` - Handle Altegio webhooks
- `GET /api/altegio/bookings` - Get bookings from Altegio

## 🤖 Telegram Bot

The system includes a Telegram bot for:
- Court booking notifications
- Event updates
- User interactions

## 🔄 Background Services

The application includes several background services for:
- Automated booking processing
- Event notifications
- Data synchronization with external services

## 🔒 Security

- Environment-based configuration for sensitive data
- Secure database connections
- Input validation and sanitization
- Bearer token authentication

## 🧪 Testing

Run the tests using:
```bash
dotnet test
```

## 📝 API Documentation

API documentation is available through Swagger UI at:
```
http://localhost:5152/swagger
```

## 🔄 Deployment

1. Build and publish the application:
```bash
dotnet publish -c Release
```

2. Copy the published files to the server:
```bash
scp -r bin/Release/net9.0/publish/* user@server:/root/padel-in-dubai/PadelInDubaiBE/Release/
```

3. Set up the systemd service:
```bash
sudo cp padelindubai.service /etc/systemd/system/
sudo systemctl daemon-reload
sudo systemctl enable padelindubai
sudo systemctl start padelindubai
```

4. Monitor the service:
```bash
sudo systemctl status padelindubai
sudo journalctl -u padelindubai -f
```

## 📈 Monitoring

- Application logs are stored in the `Logs` directory
- Systemd journal for service monitoring
- Performance monitoring through built-in metrics
- Error tracking and logging

## 🤝 Contributing

1. Fork the repository
2. Create your feature branch
3. Commit your changes
4. Push to the branch
5. Create a Pull Request

## 📄 License

This project is licensed under the MIT License - see the LICENSE file for details.
