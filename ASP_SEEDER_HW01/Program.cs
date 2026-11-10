
using Microsoft.Data.Sqlite;
using System.Net;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

string styles = """
    <meta name="viewport" content="width=device-width, initial-scale=1">

    <style>
        * {
            box-sizing: border-box;
        }

        body {
            margin: 0;
            font-family: Arial, sans-serif;
            background: #f4f6f9;
            color: #252a34;
        }

        header {
            background: #202938;
            color: white;
            padding: 20px 8%;
        }

        header a {
            color: white;
            text-decoration: none;
            font-size: 24px;
            font-weight: bold;
        }

        main {
            max-width: 1200px;
            margin: 35px auto;
            padding: 0 20px;
        }

        h1 {
            margin-bottom: 8px;
        }

        .subtitle {
            color: #707784;
            margin-bottom: 30px;
        }

        .product-grid {
            display: grid;
            grid-template-columns: repeat(auto-fill, minmax(220px, 1fr));
            gap: 22px;
        }

        .product-card {
            background: white;
            border-radius: 12px;
            overflow: hidden;
            box-shadow: 0 3px 12px #00000010;
            transition: transform 0.2s;
        }

        .product-card:hover {
            transform: translateY(-5px);
        }

        .product-card img {
            width: 100%;
            height: 180px;
            object-fit: cover;
        }

        .product-info {
            padding: 16px;
        }

        .category {
            color: #777f8c;
            font-size: 13px;
        }

        .price {
            color: #247a55;
            font-size: 20px;
            font-weight: bold;
            margin: 12px 0;
        }

        .button {
            display: inline-block;
            background: #356ae6;
            color: white;
            padding: 10px 16px;
            border-radius: 6px;
            text-decoration: none;
        }

        .button:hover {
            background: #2453c5;
        }

        .details-card {
            background: white;
            border-radius: 12px;
            padding: 25px;
            display: grid;
            grid-template-columns: 1fr 1fr;
            gap: 30px;
            box-shadow: 0 3px 12px #00000010;
        }

        .details-card img {
            width: 100%;
            border-radius: 10px;
        }

        .details-info {
            align-self: center;
        }

        .description {
            color: #666;
            line-height: 1.6;
        }

        @media (max-width: 650px) {
            .details-card {
                grid-template-columns: 1fr;
            }
        }
    </style>
    """;


// Random generated images
string GetImageUrl(string name, string category, int id)
{
    return $"https://picsum.photos/seed/product{id}/640/420";
}

string Page(string content)
{
    return $"""
        <!DOCTYPE html>
        <html>
        <head>
            <title>My Online Shop</title>
            {styles}
        </head>
        <body>
            <header>
                <a href="/">My Online Shop</a>
            </header>
            <main>
                {content}
            </main>
        </body>
        </html>
        """;
}


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

    html.Append("<h1>Our Products</h1>");
    html.Append("<p class='subtitle'>Discover our collection of quality products.</p>");
    html.Append("<div class='product-grid'>");

    while (reader.Read())
    {
        int id = reader.GetInt32(0);
        string name = WebUtility.HtmlEncode(reader.GetString(1));
        double price = reader.GetDouble(2);
        string category = WebUtility.HtmlEncode(reader.GetString(3));

        string image = GetImageUrl(name, category, id);

        html.Append("<div class='product-card'>");

        html.Append($"""
                     <img src="{image}" alt="{name}" loading="lazy">
                     """);

        html.Append("<div class='product-info'>");

        html.Append($"<div class='category'>{category}</div>");
        html.Append($"<h3>{name}</h3>");
        html.Append($"<div class='price'>${price:F2}</div>");
        html.Append($"<a class='button' href='/products/{id}'>View Details</a>");

        html.Append("</div>");
        html.Append("</div>");
    }

    html.Append("</div>");

    return Results.Content(Page(html.ToString()), "text/html");
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

    string image = GetImageUrl(name, category, id);

    var html = new StringBuilder();

    html.Append("<h1>Product Details</h1>");
    html.Append("<p class='subtitle'>Learn more about this product.</p>");

    html.Append("<div class='details-card'>");

    html.Append($"""
                 <img src="{image}" alt="{name}">
                 """);

    html.Append("<div class='details-info'>");

    html.Append($"<div class='category'>{category}</div>");
    html.Append($"<h2>{name}</h2>");
    html.Append($"<p class='description'>{description}</p>");
    html.Append($"<div class='price'>${price:F2}</div>");
    html.Append("<a class='button' href='/'>Back to Products</a>");

    html.Append("</div>");
    html.Append("</div>");

    return Results.Content(Page(html.ToString()), "text/html");
});

app.Run();
