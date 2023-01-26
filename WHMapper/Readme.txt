#dotnet tool install -g dotnet-ef
dotnet tool update -g dotnet-ef
dotnet ef database update


#Docker command for Db postgresql

docker run --name db -e POSTGRES_USER=postgres -e POSTGRES_PASSWORD=secret -e POSTGRES_DB=whmapper -p 5432:5432 -d postgres
