FROM microsoft/dotnet:2.1-sdk AS build-env

WORKDIR /app

COPY GrandNode.sln .
COPY Grand.Core/Grand.Core.csproj Grand.Core/Grand.Core.csproj
COPY Grand.Data/Grand.Data.csproj Grand.Data/Grand.Data.csproj
COPY Grand.Framework/Grand.Framework.csproj Grand.Framework/Grand.Framework.csproj
COPY Grand.Services/Grand.Services.csproj Grand.Services/Grand.Services.csproj
COPY Grand.Web/Grand.Web.csproj Grand.Web/Grand.Web.csproj
COPY Plugins/Grand.Plugin.DiscountRequirements.Standard/Grand.Plugin.DiscountRequirements.Standard.csproj Plugins/Grand.Plugin.DiscountRequirements.Standard/Grand.Plugin.DiscountRequirements.Standard.csproj
COPY Plugins/Grand.Plugin.ExchangeRate.McExchange/Grand.Plugin.ExchangeRate.McExchange.csproj Plugins/Grand.Plugin.ExchangeRate.McExchange/Grand.Plugin.ExchangeRate.McExchange.csproj
COPY Plugins/Grand.Plugin.ExternalAuth.Facebook/Grand.Plugin.ExternalAuth.Facebook.csproj Plugins/Grand.Plugin.ExternalAuth.Facebook/Grand.Plugin.ExternalAuth.Facebook.csproj
COPY Plugins/Grand.Plugin.Feed.GoogleShopping/Grand.Plugin.Feed.GoogleShopping.csproj Plugins/Grand.Plugin.Feed.GoogleShopping/Grand.Plugin.Feed.GoogleShopping.csproj
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
COPY Plugins/Grand.Plugin.Widgets.Slider/Grand.Plugin.Widgets.Slider.csproj Plugins/Grand.Plugin.Widgets.Slider/Grand.Plugin.Widgets.Slider.csproj

# Copy everything else and build
COPY . ./
RUN dotnet publish Grand.Web -c Release -o out
RUN dotnet build Plugins/Grand.Plugin.DiscountRequirements.Standard
RUN dotnet build Plugins/Grand.Plugin.ExchangeRate.McExchange
RUN dotnet build Plugins/Grand.Plugin.ExternalAuth.Facebook
RUN dotnet build Plugins/Grand.Plugin.Feed.GoogleShopping
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
RUN dotnet build Plugins/Grand.Plugin.Widgets.Slider

# Build runtime image
FROM microsoft/dotnet:2.1-aspnetcore-runtime 
RUN apt-get update && \
  apt-get -y install libgdiplus
RUN ln -s /lib/x86_64-linux-gnu/libdl.so.2 /lib/x86_64-linux-gnu/libdl.so

WORKDIR /app
COPY --from=build-env /app/Grand.Web/out/ .
COPY --from=build-env /app/Grand.Web/Plugins/ ./Plugins/

VOLUME /app/App_Data /app/wwwroot /app/Plugins /app/Themes

ENTRYPOINT ["dotnet", "Grand.Web.dll"]
