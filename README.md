# Identity Server

## Adjust sqlite DB by CLI

1. install ef tools (if needed):
	dotnet tool install -g dotnet-ef

	1.1 update ef tools:
		dotnet tool update -g dotnet-ef --version <versionValue>

2. create migrations
	2.1 
		Add-Migration <name> -c PersistedGrantDbContext -o ./Migrations/PersistedGrantDb
		Add-Migration <name> -c ConfigurationDbContext -o ./Migrations/ConfigurationDb
		Add-Migration <name> -c AppDbContext -o ./Migrations/AppDb

3 enable migrations
	3.1
		Update-Database -Context PersistedGrantDbContext



# Deploy to docker

## Rebuild
docker-compose -f docker-compose.yml build

## Push
docker push ukrgerri4/identity-server:latest

## On server
docker-compose pull
docker-compose up -d