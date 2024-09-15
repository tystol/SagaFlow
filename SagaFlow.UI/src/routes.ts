import Commands from './routes/commands.svelte';
import Resources from './routes/resources.svelte';
import NotFound from './lib/NotFound.svelte';
import PageRecords from "./components/records/PageRecords.svelte";
import PageWidget from "./components/widgets/PageWidget.svelte";
import {wrap} from 'svelte-spa-router/wrap'

const routes = {
    '/': NotFound,
    '/resources': wrap({
        component: PageRecords,
        props: {
            type: 'resources'
        }
    }),
    '/commands': wrap({
        component: PageRecords,
        props: {
            type: 'commands'
        }
    }),
    '/widget': wrap({
        component: PageWidget,
        props: {
            widgetUrl: "/sagaflow/schema/example-widget"
        }
    }),
    // The catch-all route must always be last
    '*': NotFound
};

export default routes
