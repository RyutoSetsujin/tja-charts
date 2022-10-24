import Component from "../framework/Component";
import { RouteLink } from "../framework/RouteButton";
import { CacheMap } from "../interfaces/Cache";
import BeatmapPlayerPage from "../pages/BeatmapPlayerPage";

export default class DtxPreview extends Component {
    Image = <img /> as HTMLImageElement
    Title = <h3 />
    Description = <h5 />
    Download = <a href="https://jumprocks1.github.io/drum-game" target="_blank" rel="noreferrer noopener">Download</a>
    Date = <span />
    DownloadLine = <div>
        {this.Download} - {this.Date}
    </div>
    Preview = <RouteLink page={BeatmapPlayerPage}>Preview Sheet Music</RouteLink>


    constructor() {
        super();
        this.HTMLElement = <div id="map-preview">
            {this.Image}
            {this.Title}
            {this.Description}
            {this.DownloadLine}
            {this.Preview}
        </div>
    }

    SetMap(map: CacheMap) {
        this.Image.src = map.ImageUrl ?? "";
        this.Title.textContent = `${map.Artist} - ${map.Title}`;
        this.Description.textContent = `${map.BpmString} BPM - ${map.DifficultyString}`;
        this.Date.textContent = map.Date ?? "";
        const fileLink = map.FileName.substring(0, map.FileName.lastIndexOf("."));
        (this.Preview.Component as RouteLink).Parameters = [fileLink]
    }
}