// import type {PageLoad} from "./$types";
import type {Config, Setup} from "$lib/types.d";

const setup: Setup = {
    apiRoot: "https://localhost:7172",
    apiPath: "sagaflow",
    webRoot: "/",
};
setup.apiRoot = setup.apiRoot.replace(/\/$/, "");
setup.webRoot = setup.webRoot.replace(/\/$/, "");

export const load = () => ({
    setup,
    config: fetch(`${setup.apiRoot}/sagaflow`).then(result => result.ok ? result.json() : Promise.reject(result)) as Promise<Config>
});

export const ssr = false;