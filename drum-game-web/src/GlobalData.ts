import { fetchEndpoint } from "./api/network";
import Cache from "./interfaces/Cache";
import SMuFL from "./interfaces/SMuFL";

interface CacheDef {
    mapList: Cache
    bravura: SMuFL
}

class GlobalData {
    constructor() {
    }

    GlobalCache: { [K in keyof CacheDef]?: Promise<CacheDef[K]> } = {}

    private LoadCacheItem<K extends keyof CacheDef>(key: K, url: string) {
        if (!this.GlobalCache[key]) {
            // @ts-ignore
            this.GlobalCache[key] = fetchEndpoint<CacheDef[K]>(url).then(r => {
                if (r) return r;
                throw new Error(`Failed to load ${key}`)
            })
        }
        return this.GlobalCache[key]!;
    }

    async LoadMapList() {
        const loaded = "mapList" in this.GlobalCache;
        const r = await this.LoadCacheItem("mapList", "/maps.json");
        if (!loaded) { // just do some basic post processing here
            const maps = r.Maps;
            for (const key in maps) {
                const map = maps[key];
                map.FileName = key;
                map.Id ??= key;
            }
        }
        return r;
    }
    LoadBravura() {
        return this.LoadCacheItem("bravura", "/bravura_metadata.json")
    }
}

export default new GlobalData();