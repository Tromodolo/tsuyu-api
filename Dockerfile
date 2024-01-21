FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 7000

# Defaults to 7000 if not set
# Make sure to change exposed port above if this is changed
ENV Port=7000

#JWT Config, key size must be 512 bits long
ENV JwtIssuer=""
ENV JwtAudience=""
ENV JwtKey=""

# MariaDb connection string
ENV DbConnectionString=""

# What domain should get returned in the upload request
# {BaseUrl}/{FileName}
ENV BaseUrl="localhost:7000"

# Whether it should be possible to register
# Default to false
# A value of false will allow a single user to register
# if there are no existing users (creating a superadmin user)
ENV RegisterEnabled=false

# Max file size in bytes
# Defaults to 100 MB
ENV MaxFileSizeBytes=104857600

# Length of the generated file names when uploading
# Defaults to 12 characters long
ENV FileNameLength=12

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["tsuyu.csproj", "."]
RUN dotnet restore "./tsuyu.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "tsuyu.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "tsuyu.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "tsuyu.dll"]