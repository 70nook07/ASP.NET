
using Microsoft.Data.Sqlite;
using System.Net;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

string connectionString = "Data Source=shop.db";

// Database init + seed data
using (var connection = new SqliteConnection(connectionString))
{
    connection.Open();

    var command = connection.CreateCommand();

    command.CommandText = @"
        CREATE TABLE IF NOT EXISTS Categories (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            Name TEXT NOT NULL
        );

        CREATE TABLE IF NOT EXISTS Products (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            Name TEXT NOT NULL,
            Description TEXT NOT NULL,
            Price REAL NOT NULL,
            CategoryId INTEGER NOT NULL,
            FOREIGN KEY (CategoryId) REFERENCES Categories(Id)
        );
    ";

    command.ExecuteNonQuery();

    command.CommandText = "SELECT COUNT(*) FROM Products";

    if (Convert.ToInt32(command.ExecuteScalar()) == 0)
    {
        string[] categories =
        {
            "Electronics", "Computers", "Phones", "Clothing",
            "Shoes", "Books", "Furniture", "Sports", "Toys",
            "Accessories"
        };

        string[][] products =
        {
            new[] { "Headphones", "Monitor", "Keyboard", "Mouse", "Speaker" },
            new[] { "Laptop", "Desktop PC", "Graphics Card", "RAM", "SSD" },
            new[] { "Smartphone", "Tablet", "Phone Case", "Charger", "Power Bank" },
            new[] { "T-Shirt", "Jeans", "Jacket", "Sweater", "Shorts" },
            new[] { "Sneakers", "Boots", "Sandals", "Running Shoes", "Slippers" },
            new[] { "Novel", "Dictionary", "Cookbook", "Biography", "Textbook" },
            new[] { "Chair", "Desk", "Sofa", "Bookshelf", "Coffee Table" },
            new[] { "Football", "Basketball", "Tennis Racket", "Dumbbells", "Yoga Mat" },
            new[] { "Toy Car", "Teddy Bear", "Lego Set", "Puzzle", "Board Game" },
            new[] { "Watch", "Wallet", "Backpack", "Sunglasses", "Belt" }
        };

        for (int i = 0; i < categories.Length; i++)
        {
            command.CommandText =
                "INSERT INTO Categories (Name) VALUES (@name)";

            command.Parameters.Clear();
            command.Parameters.AddWithValue("@name", categories[i]);
            command.ExecuteNonQuery();

            command.CommandText = "SELECT last_insert_rowid()";
            long categoryId = (long)command.ExecuteScalar()!;

            for (int j = 0; j < 5; j++)
            {
                command.CommandText = @"
                    INSERT INTO Products
                    (Name, Description, Price, CategoryId)
                    VALUES (@name, @description, @price, @categoryId)
                ";

                command.Parameters.Clear();
                command.Parameters.AddWithValue("@name", products[i][j]);
                command.Parameters.AddWithValue(
                    "@description",
                    $"High-quality {products[i][j].ToLower()}."
                );
                command.Parameters.AddWithValue("@price", 20 + i * 15 + j * 10);
                command.Parameters.AddWithValue("@categoryId", categoryId);

                command.ExecuteNonQuery();
            }
        }
    }
}

// Homepage
app.MapGet("/", () =>
{
    using var connection = new SqliteConnection(connectionString);
    connection.Open();

    var command = connection.CreateCommand();

    command.CommandText = @"
        SELECT Products.Id, Products.Name, Products.Price,
               Categories.Name AS Category
        FROM Products
        JOIN Categories ON Products.CategoryId = Categories.Id
        ORDER BY Products.Id
    ";

    using var reader = command.ExecuteReader();

    var html = new StringBuilder();

    html.Append("<h1>My Online Shop</h1>");
    html.Append("<h2>Our Products</h2>");
    html.Append("<p>Browse our collection of 50 products.</p>");
    html.Append("<table border='1' cellpadding='10'>");
    html.Append("<tr><th>Name</th><th>Category</th><th>Price</th><th></th></tr>");

    while (reader.Read())
    {
        int id = reader.GetInt32(0);
        string name = WebUtility.HtmlEncode(reader.GetString(1));
        double price = reader.GetDouble(2);
        string category = WebUtility.HtmlEncode(reader.GetString(3));

        html.Append("<tr>");
        html.Append($"<td>{name}</td>");
        html.Append($"<td>{category}</td>");
        html.Append($"<td>${price:F2}</td>");
        html.Append($"<td><a href='/products/{id}'>Details</a></td>");
        html.Append("</tr>");
    }

    html.Append("</table>");

    return Results.Content(html.ToString(), "text/html");
});

// Product details
app.MapGet("/products/{id:int}", (int id) =>
{
    using var connection = new SqliteConnection(connectionString);
    connection.Open();

    var command = connection.CreateCommand();

    command.CommandText = @"
        SELECT Products.Id, Products.Name, Products.Description,
               Products.Price, Categories.Name AS Category
        FROM Products
        JOIN Categories ON Products.CategoryId = Categories.Id
        WHERE Products.Id = @id
    ";

    command.Parameters.AddWithValue("@id", id);

    using var reader = command.ExecuteReader();

    if (!reader.Read())
        return Results.NotFound("Product not found.");

    string name = WebUtility.HtmlEncode(reader.GetString(1));
    string description = WebUtility.HtmlEncode(reader.GetString(2));
    double price = reader.GetDouble(3);
    string category = WebUtility.HtmlEncode(reader.GetString(4));

    var html = new StringBuilder();

    html.Append("<h1>Product Details</h1>");
    html.Append($"<h2>{name}</h2>");
    html.Append($"<p>Product ID: {id}</p>");
    html.Append($"<p>Category: {category}</p>");
    html.Append($"<p>Description: {description}</p>");
    html.Append($"<h3>Price: ${price:F2}</h3>");
    html.Append("<a href='/'>Back to homepage</a>");

    return Results.Content(html.ToString(), "text/html");
});

app.Run();
