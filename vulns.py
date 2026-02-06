import mysql.connector
import os
import subprocess
import hashlib
import pickle
import yaml
import xml.etree.ElementTree as ET

# --- 1. Hardcoded Credentials ---
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


# --- 2. SQL Injection ---
def get_user(username):
    conn = get_db_connection()
    cursor = conn.cursor()
    query = "SELECT * FROM users WHERE username = '" + username + "'"
    cursor.execute(query)
    return cursor.fetchone()


# --- 3. Command Injection (OS Command Injection) ---
def ping_host(host):
    result = os.system("ping -c 1 " + host)
    return result

def run_command(user_input):
    output = subprocess.check_output("echo " + user_input, shell=True)
    return output


# --- 4. Path Traversal ---
def read_file(filename):
    with open("/var/app/data/" + filename, "r") as f:
        return f.read()


# --- 5. Weak Hashing (MD5 / SHA1) ---
def hash_password(password):
    return hashlib.md5(password.encode()).hexdigest()

def hash_token(token):
    return hashlib.sha1(token.encode()).hexdigest()


# --- 6. Insecure Deserialization ---
def load_session(data):
    return pickle.loads(data)


# --- 7. YAML Deserialization (Arbitrary Code Execution) ---
def parse_config(yaml_content):
    return yaml.load(yaml_content)


# --- 8. XXE (XML External Entity Injection) ---
def parse_xml(xml_string):
    tree = ET.fromstring(xml_string)
    return tree


# --- 9. SSRF (Server-Side Request Forgery) ---
import requests

def fetch_url(url):
    response = requests.get(url)
    return response.text


# --- 10. Open Redirect ---
from flask import Flask, redirect, request

app = Flask(__name__)

@app.route("/redirect")
def open_redirect():
    target = request.args.get("url")
    return redirect(target)


# --- 11. XSS (Cross-Site Scripting) ---
from flask import make_response

@app.route("/greet")
def greet():
    name = request.args.get("name", "")
    html = "<h1>Hello, " + name + "!</h1>"
    return make_response(html)


# --- 12. Hardcoded Secret in JWT ---
import jwt

def generate_token(user_id):
    payload = {"user_id": user_id}
    token = jwt.encode(payload, "my-super-secret-key-12345", algorithm="HS256")
    return token
