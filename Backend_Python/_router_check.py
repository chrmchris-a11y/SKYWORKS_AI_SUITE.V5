import asyncio
from sail.api.sail_api import SAILCalculationAPIRequest, calculate_sail
from sail.models.sail_models import ARCLevel, SORAVersion

async def main():
    req = SAILCalculationAPIRequest(grc_level=3, arc_level=ARCLevel('a'), sora_version=SORAVersion.SORA_2_0)
    resp = await calculate_sail(req)
    print(resp)

asyncio.run(main())
