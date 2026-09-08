from fastapi import FastAPI, File, HTTPException, UploadFile
from fastapi.responses import JSONResponse

from classifier import InvalidImageError, MODEL_ID, MODEL_REVISION, PokemonClassifier


MAX_IMAGE_BYTES = 5 * 1024 * 1024
ALLOWED_CONTENT_TYPES = {"image/jpeg", "image/png", "image/webp"}
classifier = PokemonClassifier()


app = FastAPI(title="Pokedex Vision Service")


@app.get("/health")
async def health() -> dict[str, str]:
    return {"status": "healthy"}


@app.get("/ready")
async def ready():
    payload = {
        "ready": classifier.is_ready,
        "model": MODEL_ID,
        "modelRevision": MODEL_REVISION,
        "device": classifier.device,
        "error": classifier.load_error,
    }
    return JSONResponse(payload, status_code=200 if classifier.is_ready else 503)


@app.post("/warmup", status_code=202)
async def warmup() -> dict[str, str]:
    classifier.start_loading()
    return {"status": "loading" if not classifier.is_ready else "ready"}


@app.post("/identify")
async def identify(image: UploadFile = File(...)):
    if image.content_type not in ALLOWED_CONTENT_TYPES:
        raise HTTPException(status_code=415, detail="Upload a JPEG, PNG, or WebP image.")

    image_bytes = await image.read(MAX_IMAGE_BYTES + 1)
    if not image_bytes:
        raise HTTPException(status_code=400, detail="The image is empty.")
    if len(image_bytes) > MAX_IMAGE_BYTES:
        raise HTTPException(status_code=413, detail="The image must be 5 MB or smaller.")

    try:
        return await classifier.classify_async(image_bytes)
    except InvalidImageError as error:
        raise HTTPException(status_code=400, detail=str(error)) from error