import { writable, get } from "svelte/store";
import ApiClient    from "../utils/ApiClient";
import CommonHelper from "../utils/CommonHelper";
import sagaFlow, {defaultSagaFlowServer} from "../state/SagaFlowState";
import { property } from "lodash";

export const collections                    = writable([]);
export const activeCollection               = writable({});
export const isCollectionsLoading           = writable(false);
export const protectedFilesCollectionsCache = writable({});

let notifyChannel;

if (typeof BroadcastChannel != "undefined") {
    notifyChannel = new BroadcastChannel("collections");

    notifyChannel.onmessage = () => {
        let activeCollectionValue = get(activeCollection);
        loadCollections(activeCollectionValue?.type, activeCollectionValue?.id)
    }
}

function notifyOtherTabs() {
    notifyChannel?.postMessage("reload");
}

export function changeActiveCollectionById(collectionId) {
    collections.update((list) => {
        const found = CommonHelper.findByKey(list, "id", collectionId);

        if (found) {
            activeCollection.set(found);
        } else if (list.length) {
            activeCollection.set(list[0]);
        }

        return list;
    });
}

// add or update collection
export function addCollection(collection) {
    activeCollection.update((current) => {
        return CommonHelper.isEmpty(current?.id) || current.id === collection.id ? collection : current;
    });

    collections.update((list) => {
        CommonHelper.pushOrReplaceByKey(list, collection, "id");

        refreshProtectedFilesCollectionsCache();

        notifyOtherTabs();

        return CommonHelper.sortCollections(list);
    });
}

export function removeCollection(collection) {
    collections.update((list) => {
        CommonHelper.removeByKey(list, "id", collection.id);

        activeCollection.update((current) => {
            if (current.id === collection.id) {
                return list[0];
            }
            return current;
        });

        refreshProtectedFilesCollectionsCache();

        notifyOtherTabs();

        return list;
    });
}

// load all collections (excluding the user profile)
export async function loadCollections(type, activeId = null) {
    isCollectionsLoading.set(true);

    try {

        //let items = await ApiClient.collections.getFullList(200, { "sort": "+name" })
        //items = CommonHelper.sortCollections(items);
        let items;
        if (type === 'resources'){
            let resourceLists = await sagaFlow.getResourceLists();
            let resourceListArray = Object.entries(resourceLists).map(([id, resourceList]) => ({id, ...resourceList, type}));
            items = resourceListArray;
        }
        else if (type === 'commands'){
            let commands = await sagaFlow.getCommands();
            let commandArray = Object.entries(commands).map(([id, command]) => {
                let commandDefinition = {id, ...command, type};
                commandDefinition.schema = Object.entries(command.schema).map(([id, property]) => ({id, ...property}));
                return commandDefinition;
            });
            items = commandArray;
        }
        else {
            throw new Error(`Unknown collection type of ${type}`);
        }

        collections.set(items);

        const item = activeId && CommonHelper.findByKey(items, "id", activeId);
        if (item) {
            activeCollection.set(item);
        } else if (items.length) {
            activeCollection.set(items[0]);
        }

        refreshProtectedFilesCollectionsCache();
    } catch (err) {
        ApiClient.error(err);
    }

    isCollectionsLoading.set(false);
}

function refreshProtectedFilesCollectionsCache() {
    protectedFilesCollectionsCache.update((cache) => {
        collections.update((current) => {
            for (let c of current) {
                cache[c.id] = !!c.schema?.find((f) => f.type == "file" && f.options?.protected);
            }

            return current;
        });

        return cache;
    });
}
