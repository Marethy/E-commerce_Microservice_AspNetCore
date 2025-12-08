import os
from dataclasses import dataclass
from dotenv import load_dotenv

load_dotenv()

@dataclass
class Config:
    GRPC_PORT: int = int(os.getenv("GRPC_PORT", "50051"))
    HTTP_PORT: int = int(os.getenv("HTTP_PORT", "8001"))
    OPENAI_API_KEY: str = os.getenv("OPENAI_API_KEY", "")
    EMBEDDING_MODEL: str = os.getenv("EMBEDDING_MODEL", "text-embedding-3-small")
    LLM_MODEL: str = os.getenv("LLM_MODEL", "gpt-4o-mini")
    BROWSER_HEADLESS: bool = os.getenv("BROWSER_HEADLESS", "true").lower() == "true"
    LOG_LEVEL: str = os.getenv("LOG_LEVEL", "INFO")

config = Config()

def get_llm():
    from langchain_openai import ChatOpenAI
    return ChatOpenAI(
        model=config.LLM_MODEL,
        api_key=config.OPENAI_API_KEY,
        temperature=0
    )
