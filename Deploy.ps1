dotnet publish | Out-Null
scp -r .\HomePage\bin\Release\net8.0\publish\ admin@192.168.0.40:/home/HomePage | Out-Null