import MapCarousel, { CarouselState } from "../selector/MapCarousel";
import PageComponent from "../framework/PageComponent";
import Router from "../framework/Router";
import GlobalData from "../GlobalData";
import { CacheMap } from "../interfaces/Cache";
import BeatmapPlayerPage from "./BeatmapPlayerPage";
import MapPreview from "../components/MapPreview";

export default class MapSelectorPage extends PageComponent {
    static Route = ".*"
    static RouteUrl = ""
    static PageId = "map-selector-page"

    AfterParent() {
        super.AfterParent();

        const carousel = new MapCarousel();

        const preview = new MapPreview();
        carousel.OnMapChange = e => {
            preview.SetMap(e);
        }

        this.Add(preview);
        this.Add(carousel);

        GlobalData.LoadMapList().then(maps => {
            if (!this.Alive) return;
            const o: CacheMap[] = Object.values(maps.Maps);
            carousel.SetItems(o.sort((a, b) => a.Difficulty - b.Difficulty));
        })
    }

    AfterRemove() {
        super.AfterRemove();
        this.ChildrenAfterRemove();
    }

    LoadMap(map: CacheMap | undefined) {
        if (!map || !this.Alive) return;
        GlobalData.LoadBravura(); // preload
        const ext = ".bjson";
        const target = map.FileName.endsWith(ext) ? map.FileName.substring(0, map.FileName.length - ext.length) : map.FileName
        this.FindParent(Router).NavigateTo({ page: BeatmapPlayerPage, parameters: [target] }) // this will kill us
    }
}