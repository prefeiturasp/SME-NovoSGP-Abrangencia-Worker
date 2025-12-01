FROM mcr.microsoft.com/dotnet/runtime:9.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/SME.NovoSGP.Abrangencia.Worker/SME.NovoSGP.Abrangencia.Worker.csproj", "src/SME.NovoSGP.Abrangencia.Worker/"]
COPY ["src/SME.NovoSGP.Abrangencia.IoC/SME.NovoSGP.Abrangencia.IoC.csproj", "SME.NovoSGP.Abrangencia.IoC/"]
COPY ["src/SME.NovoSGP.Abrangencia.Aplicacao/SME.NovoSGP.Abrangencia.Aplicacao.csproj", "SME.NovoSGP.Abrangencia.Aplicacao/"]
COPY ["src/SME.NovoSGP.Abrangencia.Infra/SME.NovoSGP.Abrangencia.Infra.csproj", "SME.NovoSGP.Abrangencia.Infra/"]
COPY ["src/SME.NovoSGP.Abrangencia.Dados/SME.NovoSGP.Abrangencia.Dados.csproj", "SME.NovoSGP.Abrangencia.Dados/"]
COPY ["src/SME.NovoSGP.Abrangencia.Dominio/SME.NovoSGP.Abrangencia.Dominio.csproj", "SME.NovoSGP.Abrangencia.Dominio/"]
RUN dotnet restore "./src/SME.NovoSGP.Abrangencia.Worker/SME.NovoSGP.Abrangencia.Worker.csproj"
COPY . .
WORKDIR "/src/src/SME.NovoSGP.Abrangencia.Worker"
RUN dotnet build "./SME.NovoSGP.Abrangencia.Worker.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
RUN dotnet publish "./SME.NovoSGP.Abrangencia.Worker.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENV TZ=America/Sao_Paulo

RUN apt-get update \
  && apt-get install -yq tzdata locales -y \
  && dpkg-reconfigure --frontend noninteractive tzdata \ 
  && locale-gen en_US.UTF-8 \
  && apt-get install -y --allow-unauthenticated \
  libc6-dev \
  libgdiplus \
  libx11-dev \
  && rm -rf /var/lib/apt/lists/*

ENTRYPOINT ["dotnet", "SME.NovoSGP.Abrangencia.Worker.dll"]