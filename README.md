# PM_TakeHomeProject

### How to build this project

First open up Powershell to the APITest/APITest folder, you should see the Dockerfile

Then run ```docker build -t take_home_project .``` (Don't foreget the period)

Once that completes run ```docker run -d -p 8080:80 --name take_home_project_app take_home_project```

Once that completes the service should be up and running on http://localhost:8080/api/Leaderboards/

### The available APIs for this project are
GET http://localhost:8080/api/Leaderboards/GetAllLeaderboards

POST http://localhost:8080/api/Leaderboards/CreateLeaderboard

POST http://localhost:8080/api/Leaderboards/AddEntriesToLeaderboard

POST http://localhost:8080/api/Leaderboards/GetTopNLeaderboardEntries

### TDD for this project
https://docs.google.com/document/d/18MUcpdi8pxTwegQWtXNvYm_R1rfaU13xWdhY-fP2VlQ/edit

### Unit tests
Unit tests can be run by opening up the solution file in visual studio and running unit tests on the project
