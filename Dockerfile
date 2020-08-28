FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build-env

WORKDIR /app

COPY GrandNode.sln .
COPY Grand.Domain/Grand.Domain.csproj Grand.Domain/Grand.Domain.csproj
COPY Grand.Core/Grand.Core.csproj Grand.Core/Grand.Core.csproj
COPY Grand.Framework/Grand.Framework.csproj Grand.Framework/Grand.Framework.csproj
COPY Grand.Services/Grand.Services.csproj Grand.Services/Grand.Services.csproj
COPY Grand.Web/Grand.Web.csproj Grand.Web/Grand.Web.csproj
COPY Plugins/Grand.Plugin.DiscountRequirements.Standard/Grand.Plugin.DiscountRequirements.Standard.csproj Plugins/Grand.Plugin.DiscountRequirements.Standard/Grand.Plugin.DiscountRequirements.Standard.csproj
COPY Plugins/Grand.Plugin.Misc.EuropaCheckVat/Grand.Plugin.Misc.EuropaCheckVat.csproj Plugins/Grand.Plugin.Misc.EuropaCheckVat/Grand.Plugin.Misc.EuropaCheckVat.csproj
COPY Plugins/Grand.Plugin.ExchangeRate.McExchange/Grand.Plugin.ExchangeRate.McExchange.csproj Plugins/Grand.Plugin.ExchangeRate.McExchange/Grand.Plugin.ExchangeRate.McExchange.csproj
COPY Plugins/Grand.Plugin.ExternalAuth.Facebook/Grand.Plugin.ExternalAuth.Facebook.csproj Plugins/Grand.Plugin.ExternalAuth.Facebook/Grand.Plugin.ExternalAuth.Facebook.csproj
COPY Plugins/Grand.Plugin.ExternalAuth.Google/Grand.Plugin.ExternalAuth.Google.csproj Plugins/Grand.Plugin.ExternalAuth.Google/Grand.Plugin.ExternalAuth.Google.csproj
COPY Plugins/Grand.Plugin.Feed.GoogleShopping/Grand.Plugin.Feed.GoogleShopping.csproj Plugins/Grand.Plugin.Feed.GoogleShopping/Grand.Plugin.Feed.GoogleShopping.csproj
COPY Plugins/Grand.Plugin.Payments.BrainTree/Grand.Plugin.Payments.BrainTree.csproj Plugins/Grand.Plugin.Payments.BrainTree/Grand.Plugin.Payments.BrainTree.csproj
COPY Plugins/Grand.Plugin.Payments.CashOnDelivery/Grand.Plugin.Payments.CashOnDelivery.csproj Plugins/Grand.Plugin.Payments.CashOnDelivery/Grand.Plugin.Payments.CashOnDelivery.csproj
COPY Plugins/Grand.Plugin.Payments.CheckMoneyOrder/Grand.Plugin.Payments.CheckMoneyOrder.csproj Plugins/Grand.Plugin.Payments.CheckMoneyOrder/Grand.Plugin.Payments.CheckMoneyOrder.csproj
COPY Plugins/Grand.Plugin.Payments.PayInStore/Grand.Plugin.Payments.PayInStore.csproj Plugins/Grand.Plugin.Payments.PayInStore/Grand.Plugin.Payments.PayInStore.csproj
COPY Plugins/Grand.Plugin.Payments.PayPalStandard/Grand.Plugin.Payments.PayPalStandard.csproj Plugins/Grand.Plugin.Payments.PayPalStandard/Grand.Plugin.Payments.PayPalStandard.csproj
COPY Plugins/Grand.Plugin.Shipping.ByWeight/Grand.Plugin.Shipping.ByWeight.csproj Plugins/Grand.Plugin.Shipping.ByWeight/Grand.Plugin.Shipping.ByWeight.csproj
COPY Plugins/Grand.Plugin.Shipping.FixedRateShipping/Grand.Plugin.Shipping.FixedRateShipping.csproj Plugins/Grand.Plugin.Shipping.FixedRateShipping/Grand.Plugin.Shipping.FixedRateShipping.csproj
COPY Plugins/Grand.Plugin.Shipping.ShippingPoint/Grand.Plugin.Shipping.ShippingPoint.csproj Plugins/Grand.Plugin.Shipping.ShippingPoint/Grand.Plugin.Shipping.ShippingPoint.csproj
COPY Plugins/Grand.Plugin.Tax.CountryStateZip/Grand.Plugin.Tax.CountryStateZip.csproj Plugins/Grand.Plugin.Tax.CountryStateZip/Grand.Plugin.Tax.CountryStateZip.csproj
COPY Plugins/Grand.Plugin.Tax.FixedRate/Grand.Plugin.Tax.FixedRate.csproj Plugins/Grand.Plugin.Tax.FixedRate/Grand.Plugin.Tax.FixedRate.csproj
COPY Plugins/Grand.Plugin.Widgets.GoogleAnalytics/Grand.Plugin.Widgets.GoogleAnalytics.csproj Plugins/Grand.Plugin.Widgets.GoogleAnalytics/Grand.Plugin.Widgets.GoogleAnalytics.csproj
COPY Plugins/Grand.Plugin.Widgets.FacebookPixel/Grand.Plugin.Widgets.FacebookPixel.csproj Plugins/Grand.Plugin.Widgets.FacebookPixel/Grand.Plugin.Widgets.FacebookPixel.csproj
COPY Plugins/Grand.Plugin.Widgets.Slider/Grand.Plugin.Widgets.Slider.csproj Plugins/Grand.Plugin.Widgets.Slider/Grand.Plugin.Widgets.Slider.csproj

# Copy everything else and build
COPY . ./
RUN dotnet publish Grand.Web -c Release -o out
RUN dotnet build Plugins/Grand.Plugin.DiscountRequirements.Standard
RUN dotnet build Plugins/Grand.Plugin.Misc.EuropaCheckVat
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

# Build runtime image
FROM mcr.microsoft.com/dotnet/core/aspnet:3.1.0

RUN apt-get update -qq && apt-get -y install libgdiplus libc6-dev


WORKDIR /app
COPY --from=build-env /app/out/ .
COPY --from=build-env /app/Grand.Web/Plugins/ ./Plugins/

VOLUME /app/App_Data /app/wwwroot /app/Plugins /app/Themes

RUN chmod 755 /app/Rotativa/Linux/wkhtmltopdf

CMD ["dotnet", "Grand.Web.dll"]
