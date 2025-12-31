import os
from dotenv import load_dotenv

load_dotenv()

ELASTICSEARCH_URL = os.getenv("ELASTICSEARCH_URL", "http://elasticsearch:9200")
ELASTICSEARCH_USER = os.getenv("ELASTICSEARCH_USER", "elastic")
ELASTICSEARCH_PASSWORD = os.getenv("ELASTICSEARCH_PASSWORD", "admin")
ELASTICSEARCH_INDEX = os.getenv("ELASTICSEARCH_INDEX", "products")

IMG_MODEL_NAME = "clip-ViT-B-32"
TEXT_MODEL_NAME = "sentence-transformers/clip-ViT-B-32-multilingual-v1"
RRF_K = 60
