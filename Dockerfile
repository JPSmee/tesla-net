FROM microsoft/aspnetcore-build:2.0.0

ENV DOTNET_SKIP_FIRST_TIME_EXPERIENCE=true

WORKDIR /work

# Copy just the solution and proj files to make best use of docker image caching
COPY ./tesla-net.sln .
COPY ./src/Tesla.NET/Tesla.NET.csproj ./src/Tesla.NET/Tesla.NET.csproj
COPY ./test/Tesla.NET.Tests/Tesla.NET.Tests.csproj ./test/Tesla.NET.Tests/Tesla.NET.Tests.csproj

# Run restore on just the project files, this should cache the image after restore.
RUN dotnet restore

COPY . .

# Build to ensure the tests are their own distinct step
RUN dotnet build --no-restore -f netcoreapp2.0 -c Debug ./test/Tesla.NET.Tests/Tesla.NET.Tests.csproj

# Run unit tests
RUN dotnet test --no-restore --no-build -c Debug -f netcoreapp2.0 test/Tesla.NET.Tests/Tesla.NET.Tests.csproj
