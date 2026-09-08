import unittest

import httpx

from main import app


class VisionApiTests(unittest.IsolatedAsyncioTestCase):
    async def asyncSetUp(self):
        self.client = httpx.AsyncClient(
            transport=httpx.ASGITransport(app=app),
            base_url="http://test",
        )

    async def asyncTearDown(self):
        await self.client.aclose()

    async def test_health_does_not_require_loaded_model(self):
        response = await self.client.get("/health")

        self.assertEqual(response.status_code, 200)
        self.assertEqual(response.json(), {"status": "healthy"})

    async def test_ready_reports_unavailable_before_warmup(self):
        response = await self.client.get("/ready")

        self.assertEqual(response.status_code, 503)
        self.assertFalse(response.json()["ready"])

    async def test_identify_rejects_unsupported_media_type(self):
        response = await self.client.post(
            "/identify",
            files={"image": ("sample.txt", b"not an image", "text/plain")},
        )

        self.assertEqual(response.status_code, 415)

    async def test_identify_rejects_invalid_image_bytes(self):
        response = await self.client.post(
            "/identify",
            files={"image": ("sample.png", b"not an image", "image/png")},
        )

        self.assertEqual(response.status_code, 400)
        self.assertEqual(
            response.json()["detail"],
            "The uploaded file is not a valid supported image.",
        )


if __name__ == "__main__":
    unittest.main()