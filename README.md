# Web Application-Exam

Exam in webapplication, Group 0560

## Jam.Api – Interactive Story Game Builder

The project Jam.Api is a web application where the backend is built with ASP.NET Core Web Api and Entity Framework Core, while the frontend is built with REACT, allowing users to create, edit, and play interactive story-based games.

Each game follows a linear, branch-and-merge structure, where players make choices that lead to different scenes in between the Questions, which in the end affects the Ending Scene. The goal is to make learning fun and interactive for kids, while keeping story creation simple for teachers.

## Technology Stack

    Backend: ASP.NET Core 8 Web API (requires .NET SDK 8.0.413)

    Database: SQLite (dev)

    ORM: Entity Framework Core

    Frontend: React + Vite

    Build tools: Node.js 24.11.1, npm 11.6.2

    Architecture: API controllers → services → repository/DAL; React feature-first UI

## Setup & Installation

### Requirements

    The following software must be installed before running the system: .NET SDK 8.0 (or newer stable release) and Node.Node.js 18.0, tested on 24.11.1 (or newer stable release, which includes npm)

### How to Start the Webapplication

The project contains three key folders: api, JamReact, and Jam.Api.Tests.

### Running the backend (API)

    Start by opening a terminal. When the terminal is ready, navigate to the api folder by running:
    cd api
    This changes the terminal’s working directory to the backend project.

    Next, run:
    dotnet restore
    This command restores all required packages based on the Jam.Api.csproj file.

    If you want to access Swagger or test the API directly, you can start the backend by running:
    To run the backend, execute:
    dotnet run

    When the application starts, open the URL shown in the terminal (e.g., Now listening on: http://localhost:5173) and add /swagger, or open the full URL directly in a browser:
    http://localhost:5173/swagger

    Alternatively, press Fn + F5 to start debugging and access Swagger.

### Running the Frontend (React + Vite)

    Change the terminal’s working directory to the frontend project by running:
    cd ..
    then:
    cd JamReact

    Alternatively, open a new terminal window and run:
    cd JamReact

    Install all required frontend dependencies by executing:
    npm install

    To start both the frontend and backend in development mode, run:
    npm run dev
    This launches the web application and makes it available in the browser.

### Running the Unit Test

    To run the unit tests, navigate to the test directory:
    cd Jam.Api.Tests

    Restore test dependencies by running:
    dotnet restore

    Execute the test suite with:
    dotnet test
    This command runs all unit tests and provides a summary of the results.

## Developers Candidate Number:

    100
    125
    194
