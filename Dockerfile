FROM microsoft/dotnet:latest
ENV name SecuritiesServiceLoadTester
ENV buildconfig Release
COPY src/$name /root/$name
RUN cd /root/$name && dotnet restore && dotnet build -c $buildconfig && dotnet publish -c $buildconfig
RUN cp -rf /root/$name/bin/$buildconfig/netcoreapp1.0/publish/* /root/
ENTRYPOINT ["dotnet", "/root/SecuritiesServiceLoadTester.dll"]
CMD ["default"]
