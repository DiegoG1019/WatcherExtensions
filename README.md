# WebWatcher Extensions
These are extension projects made by me, for the original purposes I made this program for.

### Media Watchers
A set of watchers designed to keep me updated on the latest releases of certain things of my interest.

#### Erai-raws watcher
Currently, Erai-raws watcher only works on Linux Systems with transmission-cli installed. I tried to make it adjustable, but it proved too complicated for the time I can put into this project.
You can easily disable this watcher. (It's already disabled, since it's not complete)
This is doubtful to be of any particular interest to anyone but me, since it'll just be filtering out releases of specific series

#### WitchCultTranslations watcher
This project is hardwired to sink logs and WCT data into specific channels. Since I'll have it running 24/7, there's no need for you to run this in your own fork, unless you make changes to it
- The WCT Channel Id is in WitchCultTranslationsWatcher class as a `const`
- The channel in question is [WitchCultTranslations](https://t.me/WitchCultTranslations)

### Server Watchers
I have yet to implement the first one, but in my head, the first things I'll do is to make a tool I can check my personal server's status with through telegram (So I can check it from wherever I am without having to port forward, or open my already noob-designed, and rather weak server to more potential attacks)

### Extra Utilities
Miscelaenous things I have nowhere else to throw into

### Urbe Watcher
A watcher that keeps watch for when my university's shitty servers decide to let me make my schedule


## Installation
Compile the projects you fancy, dive into the bin folder, and grab the `.dll`
Simply throw it into the `extensions` folder in WebWatcher and enable it.

To enable an extension, you can either run WebWatcher so it generates the entry in `.config/settings.cfg.json` and set it to `true`
Alternatively, you can add the entry yourself by adding the `.dll`'s name manually

## Contributing
Pull requests are welcome. For major changes, please open an issue first to discuss what you would like to change.

Please make sure to update tests as appropriate.

## License
[MIT](https://choosealicense.com/licenses/mit/)
