# define base image using runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

# build image using sdk
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# copy project files and restore dependencies
COPY [ "OhunIslam.WebAPI/OhunIslam.WebAPI.csproj", "OhunIslam.WebAPI/" ]
COPY [ "OhunIslam.Radio/OhunIslam.Radio.csproj", "OhunIslam.Radio/" ]

RUN dotnet restore "OhunIslam.WebAPI/OhunIslam.WebAPI.csproj"
RUN dotnet restore "OhunIslam.Radio/OhunIslam.Radio.csproj"

# copy entire project and build
COPY . .
WORKDIR "/src/OhunIslam.WebAPI"
RUN dotnet build "OhunIslam.WebAPI.csproj" -c Release -o /app/build
WORKDIR "/src/OhunIslam.Radio"
RUN dotnet build "OhunIslam.Radio.csproj" -c Release -o /app/build

#publish the app
FROM build AS publish
WORKDIR "/src/OhunIslam.WebAPI"
RUN dotnet publish "OhunIslam.WebAPI.csproj" -c Release -o /app/publish /p:UseAppHost=false
WORKDIR "/src/OhunIslam.Radio"
RUN dotnet publish "OhunIslam.Radio.csproj" -c Release -o /app/publish /p:UseAppHost=false

#final
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT [ "dotnet", "OhunIslam.WebAPI.dll" ]