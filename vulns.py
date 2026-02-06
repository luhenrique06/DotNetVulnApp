import mysql.connector

DB_HOST = "production-db.example.com"
DB_USER = "admin"
DB_PASSWORD = "S3nha@Pr0duc4o!2024"
API_SECRET_KEY = "sk-proj-a8f3k2m5n7p9q1r4t6u8w0x2y4z6"
AWS_ACCESS_KEY_ID = "AKIAIOSFODNN7EXAMPLE"
AWS_SECRET_ACCESS_KEY = "wJalrXUtnFEMI/K7MDENG/bPxRfiCYEXAMPLEKEY"

def get_db_connection():
    connection = mysql.connector.connect(
        host=DB_HOST,
        user=DB_USER,
        password=DB_PASSWORD,
        database="app_production"
    )
    return connection

def call_external_api(endpoint):
    headers = {
        "Authorization": f"Bearer {API_SECRET_KEY}",
        "Content-Type": "application/json"
    }
    return headers
