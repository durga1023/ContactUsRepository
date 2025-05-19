
# Contact Form Web Application

This is a  ASP.NET Core MVC web application featuring client/server-side validation, Google reCAPTCHA v3 integration, AWS deployment with RDS and Elastic Beanstalk, secure secrets handling via AWS Secrets Manager, and rate-limiting protection.

---

## Architecture Overview

### MVC Design Pattern
- **Model**: `ContactViewModel.cs` defines the form structure and validation rules.
- **View**: `Contact.cshtml` renders the form and integrates client-side validation and reCAPTCHA.
- **Controller**: `ContactController.cs` handles GET/POST requests, performs model validation, and handles logic.



## Validation Strategy

**Client-Side** : Model annotations enforce rules like `Required`, `EmailAddress`, etc.

**Server-Side** : Checked using `ModelState.IsValid`and Google reCAPTCHA v3 validation to assess request legitimacy via score threshold.


## Google reCAPTCHA v3 Integration

### Implementation

- On page load, the frontend generates a reCAPTCHA token using the configured site key.
- The token is included as a hidden input in the form and submitted with the user's input.
- On the server, the `ContactController` sends the token and secret key to the Google API for validation.
- The returned score is evaluated:
- If the score is **greater than or equal to 0.5**, the request is considered valid.
- If the score is **below 0.5**, the request is rejected with a user-friendly error message.
- Both the **site key** and **secret key** are securely stored in **AWS Secrets Manager** and retrieved at runtime using the `AwsSecretsHelper` class.

### Benefits

- Fully transparent to users with no interruption or additional input required.
- Significantly reduces bot submissions and spam without degrading the user experience.
- Enables flexible handling of suspicious requests based on scoring thresholds.

- **Verification Result Logging**: via log4net


## Rate Limiting

Rate limiting is implemented to prevent abuse of the contact form by limiting how many times a user can submit data within a short time frame.

### Implementation

This application uses the built-in **ASP.NET Core Rate Limiting API** (available in .NET 8) via the `System.Threading.RateLimiting` namespace.

- A custom policy named `"ContactFormPolicy"` is defined in `Program.cs`.
- This policy restricts the number of POST requests allowed per IP within a fixed window of time.
- The rate limiter is registered with `builder.Services.AddRateLimiter(...)`.
- The `ContactController` POST action is decorated with `[EnableRateLimiting("ContactFormPolicy")]` to enforce the rule.

### Example Use

If a user exceeds the allowed number of requests (e.g., 5 per minute), they will receive a `429 Too Many Requests` HTTP response.

### Benefits

- Helps prevent bots or users from spamming the form.
- Protects backend resources and ensures fair usage.
- Works in tandem with reCAPTCHA v3 to provide layered defense.

## NuGet Packages Used

| Package                              | Purpose                         |
|--------------------------------------|---------------------------------|
| `Microsoft.AspNetCore.Mvc`           | MVC framework                   |
| `Google.reCAPTCHA`                   | reCAPTCHA validation            |
| `AspNetCoreRateLimit`                | Request throttling              |
| `Amazon.SecretsManager`              | AWS Secrets retrieval           |
| `log4net`                            | Logging system                  |


## Database Design (SQL Server on AWS RDS)

### Schema

Created using SSMS after connecting to the AWS RDS SQL Server instance:

```sql
CREATE TABLE [dbo].[Contact] (
    [Id] INT IDENTITY(1,1) PRIMARY KEY,
    [FirstName] NVARCHAR(50) NOT NULL,
    [LastName] NVARCHAR(50) NULL,
    [Email] NVARCHAR(50) NOT NULL,
    [Phone] NVARCHAR(50) NULL,
    [Zip] NVARCHAR(20) NULL,
    [City] NVARCHAR(30) NULL,
    [State] NVARCHAR(30) NULL,
    [Comments] NVARCHAR(100) NULL,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT GETDATE()
);
```

### How It Was Set Up

- Provisioned an RDS instance (SQL Server) using AWS Console.
- Connected via SQL Server Management Studio (SSMS) using RDS endpoint.
- Executed CREATE TABLE command for contact data.
- Inbound access on port 1433 configured via security group.

## AWS Configuration

### Secrets Manager
- Used for: reCAPTCHA site keys, database credentials.
- Retrieved securely using AWS SDK in `AwsSecretsHelper.cs`.
- Avoided hardcoded values for production deployment.

### IAM Roles
- IAM role attached to Elastic Beanstalk environment provided access to Secrets Manager.
- No access key ID or secret key used directly in code.

### AWS Toolkit (Visual Studio)
- Used only for publishing the application to Elastic Beanstalk.
- Environment variables configured via AWS Elastic Beanstalk console or deployment settings.


## Deployment

- Application deployed via AWS Elastic Beanstalk using Visual Studio's AWS Toolkit.
- .NET Core environment selected with proper appsettings and environment variables.

**Live Deployment**:  
http://contactapp-dev.eba-vdppmvkd.us-east-2.elasticbeanstalk.com/




## Unit Testing

- Tests for `ContactController` validate:
  - Form submission
  - reCAPTCHA result handling
  - Server-side validation logic


## Challenges Faced

- IAM Permissions: Faced deployment issues initially due to IAM role not having access to Secrets Manager.
- reCAPTCHA Issues: Key mismatches caused loading problems until the correct environment variables were set.
- Environment-Specific Settings: Setup required careful alignment between local appsettings and AWS configuration.


## Assumptions

- Application is hosted in a single AWS region.
- Only one form exists (no authentication or multiple views).
- SSMS is available for DB admin tasks.


## Architectural Decisions

- ASP.NET Core MVC used for structure and testability.
- AWS RDS used for managed DB hosting.
- Secrets Manager and IAM roles chosen for security.


## Improvements Recommended

- Add confirmation emails to submitter/admin.
- Integrate Entity Framework Core for database operations.
- Add CI/CD using GitHub Actions and AWS CLI.
