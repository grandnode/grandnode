FROM mcr.microsoft.com/dotnet/core/aspnet:3.0-buster-slim AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/core/sdk:3.0-buster AS build
WORKDIR /src
COPY ["Grand.Web/Grand.Web.csproj", "Grand.Web/"]
COPY ["Grand.Framework/Grand.Framework.csproj", "Grand.Framework/"]
COPY ["Grand.Core/Grand.Core.csproj", "Grand.Core/"]
COPY ["Grand.Services/Grand.Services.csproj", "Grand.Services/"]
COPY ["Grand.Api/Grand.Api.csproj", "Grand.Api/"]
RUN dotnet restore "Grand.Web/Grand.Web.csproj"
COPY . .
WORKDIR "/src/Grand.Web"
RUN dotnet build "Grand.Web.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Grand.Web.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
CMD ["dotnet", "Grand.Web.dll"]
