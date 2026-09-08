const streams = new WeakMap();
const requestControllers = new WeakMap();

export async function warmupAndWait(warmupUrl, readyUrl) {
    const warmupResponse = await fetch(warmupUrl, { method: "POST" });
    if (!warmupResponse.ok) throw await responseError(warmupResponse);

    for (let attempt = 0; attempt < 600; attempt += 1) {
        const response = await fetch(readyUrl, { cache: "no-store" });
        if (response.ok) return await response.json();
        await new Promise(resolve => setTimeout(resolve, 1000));
    }

    throw new Error("The recognition model did not become ready.");
}

export async function startCamera(video, canvas) {
    stopCamera(video);
    clearCanvas(canvas);

    if (!navigator.mediaDevices?.getUserMedia) {
        throw new Error("This browser does not support camera capture.");
    }

    let stream;
    try {
        stream = await navigator.mediaDevices.getUserMedia({
            audio: false,
            video: {
                facingMode: { ideal: "environment" },
                width: { ideal: 1280 },
                height: { ideal: 720 }
            }
        });
    } catch (error) {
        if (error?.name === "NotAllowedError") {
            throw new Error("Camera permission was denied.");
        }
        if (error?.name === "NotFoundError") {
            throw new Error("No camera was found.");
        }
        throw new Error("Camera access failed.");
    }
    streams.set(video, stream);
    video.srcObject = stream;
    await video.play();
}

export async function captureAndIdentify(video, canvas, endpoint) {
    if (video.readyState < HTMLMediaElement.HAVE_CURRENT_DATA) {
        throw new Error("The camera is not ready yet.");
    }

    drawSquare(video, video.videoWidth, video.videoHeight, canvas);
    stopCamera(video);
    return await identifyCanvas(canvas, endpoint);
}

export async function identifyFile(input, canvas, endpoint) {
    const file = input.files?.[0];
    if (!file) throw new Error("Choose an image first.");

    const bitmap = await createImageBitmap(file);
    try {
        drawSquare(bitmap, bitmap.width, bitmap.height, canvas);
    } finally {
        bitmap.close();
        input.value = "";
    }

    return await identifyCanvas(canvas, endpoint);
}

export function dispose(video, canvas) {
    abortRequest(canvas);
    stopCamera(video);
}

function drawSquare(source, sourceWidth, sourceHeight, canvas) {
    const sourceSize = Math.min(sourceWidth, sourceHeight);
    const sourceX = (sourceWidth - sourceSize) / 2;
    const sourceY = (sourceHeight - sourceSize) / 2;
    const outputSize = Math.min(640, sourceSize);
    canvas.width = outputSize;
    canvas.height = outputSize;
    const context = canvas.getContext("2d", { alpha: false });
    context.drawImage(
        source,
        sourceX,
        sourceY,
        sourceSize,
        sourceSize,
        0,
        0,
        outputSize,
        outputSize);
}

async function identifyCanvas(canvas, endpoint) {
    abortRequest(canvas);
    const blob = await new Promise((resolve, reject) => {
        canvas.toBlob(
            value => value ? resolve(value) : reject(new Error("Image capture failed.")),
            "image/jpeg",
            0.88);
    });
    const form = new FormData();
    form.append("image", blob, "pokemon-capture.jpg");

    const controller = new AbortController();
    requestControllers.set(canvas, controller);
    try {
        const response = await fetch(endpoint, {
            method: "POST",
            body: form,
            signal: controller.signal
        });
        if (!response.ok) throw await responseError(response);
        return await response.json();
    } finally {
        requestControllers.delete(canvas);
    }
}

async function responseError(response) {
    try {
        const payload = await response.json();
        return new Error(payload.detail ?? `Request failed with status ${response.status}.`);
    } catch {
        return new Error(`Request failed with status ${response.status}.`);
    }
}

function stopCamera(video) {
    const stream = streams.get(video) ?? video.srcObject;
    stream?.getTracks().forEach(track => track.stop());
    streams.delete(video);
    video.srcObject = null;
}

function abortRequest(key) {
    requestControllers.get(key)?.abort();
    requestControllers.delete(key);
}

function clearCanvas(canvas) {
    canvas.getContext("2d")?.clearRect(0, 0, canvas.width, canvas.height);
}