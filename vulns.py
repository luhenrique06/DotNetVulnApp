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


# --- 13. Insecure Random (Weak PRNG) ---
import random
import string

def generate_reset_token():
    return ''.join(random.choices(string.ascii_letters + string.digits, k=32))

def generate_otp():
    return random.randint(100000, 999999)


# --- 14. Eval Injection ---
def calculate(expression):
    return eval(expression)

def dynamic_import(module_name):
    exec("import " + module_name)


# --- 15. LDAP Injection ---
import ldap3

def authenticate_ldap(username, password):
    server = ldap3.Server("ldap://corp.example.com")
    conn = ldap3.Connection(server)
    conn.bind()
    search_filter = "(uid=" + username + ")"
    conn.search("dc=example,dc=com", search_filter)
    return conn.entries


# --- 16. Log Injection / Log Forging ---
import logging

logger = logging.getLogger("app")

def login(username, password):
    logger.info("Login attempt for user: " + username)
    if password == "wrong":
        logger.error("Failed login for user: " + username)
    return True


# --- 17. Regex DoS (ReDoS) ---
import re

def validate_email(email):
    pattern = r"^([a-zA-Z0-9_\.\-])+\@(([a-zA-Z0-9\-])+\.)+([a-zA-Z]{2,4})+$"
    return re.match(pattern, email)

def validate_url(url):
    pattern = r"^(https?:\/\/)?([\w\-]+\.)+[\w\-]+(\/[\w\.\-\/?%&=]*)*$"
    return re.match(pattern, url)


# --- 18. Insecure TLS / Disabled SSL Verification ---
import ssl
import urllib.request

def fetch_insecure(url):
    # Fixed: Re-enabled SSL certificate verification
    # Using default context which verifies certificates
    return urllib.request.urlopen(url).read()

def call_api_no_verify(url):
    # Fixed: Removed verify=False to enable SSL certificate verification
    # The default behavior of requests.get() is to verify SSL certificates
    return requests.get(url)


# --- 19. Insecure File Permissions ---
def write_secret_file(content):
    with open("/tmp/secrets.txt", "w") as f:
        f.write(content)
    os.chmod("/tmp/secrets.txt", 0o777)

def create_key_file(key_data):
    with open("/tmp/private_key.pem", "w") as f:
        f.write(key_data)
    os.chmod("/tmp/private_key.pem", 0o666)


# --- 20. Insecure Temp File ---
import tempfile

def save_temp_data(data):
    tmp = tempfile.mktemp()
    with open(tmp, "w") as f:
        f.write(data)
    return tmp


# --- 21. Mass Assignment / Unvalidated Input to ORM ---
from flask import jsonify

@app.route("/user/update", methods=["POST"])
def update_user():
    data = request.get_json()
    conn = get_db_connection()
    cursor = conn.cursor()
    for key, value in data.items():
        query = "UPDATE users SET " + key + " = '" + value + "' WHERE id = 1"
        cursor.execute(query)
    conn.commit()
    return jsonify({"status": "updated"})


# --- 22. Cleartext Storage of Sensitive Data ---
def store_credit_card(card_number, cvv):
    with open("payments.log", "a") as f:
        f.write(f"Card: {card_number}, CVV: {cvv}\n")
    return True

def store_password(username, password):
    with open("users.csv", "a") as f:
        f.write(f"{username},{password}\n")


# --- 23. HTTP Header Injection (CRLF Injection) ---
@app.route("/set-language")
def set_language():
    lang = request.args.get("lang", "en")
    response = make_response("Language set")
    response.headers["Content-Language"] = lang
    return response


# --- 24. Uncontrolled Format String ---
def log_event(event_type, user_input):
    log_message = "Event: %s - Details: " + user_input
    logger.info(log_message % event_type)

def format_welcome(username):
    template = "Welcome, " + username + "! Your role is: %s"
    return template


# --- 25. Race Condition (TOCTOU) ---
def safe_delete(filepath):
    if os.path.exists(filepath):
        os.remove(filepath)

def process_upload(filepath):
    if os.path.isfile(filepath):
        with open(filepath, "r") as f:
            data = f.read()
        return data


# --- 26. Hardcoded IP / Internal Network Exposure ---
INTERNAL_API = "http://192.168.1.100:8080/api/v1"
DEBUG_ENDPOINT = "http://10.0.0.5:9090/debug/pprof"
MONGO_URI = "mongodb://admin:password123@172.16.0.50:27017/production"

def get_internal_data():
    return requests.get(INTERNAL_API + "/users").json()

def health_check():
    return requests.get(DEBUG_ENDPOINT).status_code


# --- 27. Zip Slip (Arbitrary File Write via Archive Extraction) ---
import zipfile

def extract_upload(zip_path, dest_dir):
    with zipfile.ZipFile(zip_path, "r") as zf:
        for entry in zf.namelist():
            zf.extract(entry, dest_dir)
