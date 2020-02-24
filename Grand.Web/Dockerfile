FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build-env

WORKDIR /app

COPY NuGet.config /root/.config/NuGet/

COPY GrandNode.sln .

COPY . ./

RUN dotnet build Plugins/Grand.Plugin.DiscountRequirements.Standard 
RUN dotnet build Plugins/Grand.Plugin.ExchangeRate.McExchange
RUN dotnet build Plugins/Grand.Plugin.ExternalAuth.Facebook
RUN dotnet build Plugins/Grand.Plugin.ExternalAuth.Google
RUN dotnet build Plugins/Grand.Plugin.Feed.GoogleShopping
RUN dotnet build Plugins/Grand.Plugin.Payments.BrainTree
RUN dotnet build Plugins/Grand.Plugin.Payments.CashOnDelivery
RUN dotnet build Plugins/Grand.Plugin.Payments.CheckMoneyOrder
RUN dotnet build Plugins/Grand.Plugin.Payments.PayInStore
RUN dotnet build Plugins/Grand.Plugin.Payments.PayPalStandard
RUN dotnet build Plugins/Grand.Plugin.Shipping.ByWeight
RUN dotnet build Plugins/Grand.Plugin.Shipping.FixedRateShipping
RUN dotnet build Plugins/Grand.Plugin.Shipping.ShippingPoint
RUN dotnet build Plugins/Grand.Plugin.Tax.CountryStateZip
RUN dotnet build Plugins/Grand.Plugin.Tax.FixedRate
RUN dotnet build Plugins/Grand.Plugin.Widgets.GoogleAnalytics
RUN dotnet build Plugins/Grand.Plugin.Widgets.FacebookPixel
RUN dotnet build Plugins/Grand.Plugin.Widgets.Slider

# publish project
RUN dotnet publish Grand.Web -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/core/aspnet:3.1 

WORKDIR /app
COPY --from=build-env /app/out/ .

VOLUME /app/App_Data /app/wwwroot /app/Plugins /app/Themes

ENTRYPOINT ["dotnet", "Grand.Web.dll"]

