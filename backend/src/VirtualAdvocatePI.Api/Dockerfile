FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY backend/src/VirtualAdvocatePI.Api/VirtualAdvocatePI.Api.csproj backend/src/VirtualAdvocatePI.Api/
RUN dotnet restore backend/src/VirtualAdvocatePI.Api/VirtualAdvocatePI.Api.csproj

COPY . .
RUN dotnet publish backend/src/VirtualAdvocatePI.Api/VirtualAdvocatePI.Api.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "VirtualAdvocatePI.Api.dll"]
