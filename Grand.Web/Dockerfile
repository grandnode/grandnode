FROM microsoft/dotnet:2.1-aspnetcore-runtime
ARG source
WORKDIR /app
EXPOSE 80
COPY ${source:-obj/Docker/publish} .

RUN apt-get update && \
  apt-get -y install libgdiplus
RUN ln -s /lib/x86_64-linux-gnu/libdl.so.2 /lib/x86_64-linux-gnu/libdl.so

ENTRYPOINT ["dotnet", "Grand.Web.dll"]
