# .NET 8 MVC Contact Application

## Overview

This project is a migration of the original ASP.NET WebForms “Contact Us” application into a modern .NET 8 MVC architecture. It retains the core functionality including Google reCAPTCHA v3 validation, contact form submissions, and database logging while improving structure, security, and scalability.

### MVC Architecture
- Migrated from WebForms to a clean Model-View-Controller structure.
- Used Razor views for UI, Controllers for logic, and Models for data representation.
- Ensured separation of concerns with repository and service layers.

### Google reCAPTCHA v3 Integration
- Client-side integration using JavaScript from the reCAPTCHA Admin Console (https://www.google.com/recaptcha/admin).
- 
- 

### AWS RDS and Secrets Manager
- AWS RDS used for secure, scalable database management.
- Database credentials and sensitive keys are not hard-coded, fetched securely from AWS Secrets Manager using the AWS SDK.

### Security Enhancements
- Validated all inputs on both client (HTML5 and JavaScript) and server (DataAnnotations and custom validators).
- Implemented output sanitization to prevent XSS attacks.
- HTTPS redirection enabled and detailed error responses disabled in production.
- CSRF protection implemented using AntiForgery tokens.

### Rate Limiting
- Middleware-based rate limiting using AspNetCoreRateLimit to prevent abuse and brute-force attempts.
- Configurable via appsettings.json.

### Logging with log4net
- Extended log4net configuration to include error, warning, and usage logs.
- Logs are stored in local files and can be extended for cloud storage.

## Validation and Testing

- Client-side: HTML5 required fields, input patterns, and reCAPTCHA token handling.
- Server-side: ModelState validation, custom model binding, and error messaging.
- Unit testing implemented using xUnit.

## Deployment





## Screenshots

### User Interface (UI)
The contact form UI built using Razor Views in the MVC architecture.

![UI Screenshot](Screenshots/ui.png)

### Application Logs
Example of log4net logs recording submission data and errors.



### Google reCAPTCHA Dashboard
The reCAPTCHA v3 dashboard showing score analytics and request volume from the Google Admin Console.


### reCAPTCHA Error Scenario
This screenshot captures how the application responds when reCAPTCHA validation fails. In this case, the threshold was artificially raised to 0.99 to simulate bot-like traffic.

![reCAPTCHA Error at High Score](Screenshots/captcha_error_highscore.png)

### Post-Submission Thank You Page
A confirmation view that users see upon successfully submitting the contact form.

![Thank You Page](Screenshots/thankyou.png)
