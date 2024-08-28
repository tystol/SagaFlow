import Commands from './routes/commands.svelte';
import Resources from './routes/resources.svelte';
import NotFound from './lib/NotFound.svelte';
import PageRecords from "./components/records/PageRecords.svelte";

const routes = {
    '/': NotFound,
    '/commands/:id': Commands,
    '/resources/:id': Resources,
    '/collections': PageRecords,
    // The catch-all route must always be last
    '*': NotFound
};

export default routes
