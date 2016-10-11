FROM microsoft/dotnet:latest
ENV name SecuritiesServiceLoadTester
COPY src/$name /root/$name
RUN cd /root/$name && dotnet restore && dotnet build && dotnet publish
RUN cp -rf /root/$name/bin/Debug/netcoreapp1.0/publish/* /root/
CMD dotnet /root/${name}.dll
