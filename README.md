# Stellar

Stellar is a tiny client for interacting with Cosmos DB. It is still a work in progress and not recommened for production use. The goal is to be able to write Linq to Cosmos SQL and store and retieve documents in the easiest way possible. If you are looking for every configurable option for Cosmos DB, this is not the client for you.

## How to use

Create the client;
```csharp
var docs = new CosmosDbAccount("https://yourdb.documents.azure.com:443", "apikey", "dbname", "collectionname").Documents; 
```

Query documents using LINQ
```csharp
var someDocs = docs.Query<Contact>().Where(x => x.Name == "Steve");
```

Create or Update.
```csharp
var contact = new Contact{ /*... populate object */ };
docs.Store(contact.Id, contact);
```

Get an object, when you already know the id.
```csharp
var contact = docs.Get<Contact>(id);
```

Delete an object.
```csharp
docs.Delete(id);
```

## Nuget

Download from Nuget.

[https://www.nuget.org/packages/stellar/](https://www.nuget.org/packages/stellar/)


## Collections 

For small apps you probably only want one collection because that is how Cosmos DB is billed, request units on a given collection. If you separate every object into its own collection then a small app could end up with a couple hundred dollars per month of Cosmos DB charges when one collection would be fine. 

If you use one collection then we could potentially run into the same name issue. Since all documents are in the same collection what if you have 2 objects with the same property name? 

```csharp
class TestObject {
    public string Name { get; set; }
}

class AnotherObject {
    public string Name { get; set; }
}
```

A query to cosmos would look like this.
```sql
SELECT * FROM docs WHERE Name = "VeryCommonName"
```

This will bring back documents that are TestObjects and AnotherObjects. To solve this we inject the type name into the document and then uses it when querying.

```csharp
var query = docs.Query<TestObject>().Where(x => x.Name = "VeryCommonName");
```
This LINQ query will generate something like this.
```sql
SELECT * FROM TestObject t WHERE t.Name = "VeryCommonName" AND t._type = "Assembly.Namespace.TestObject"
```

