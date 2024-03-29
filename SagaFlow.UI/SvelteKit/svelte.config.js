import adapter from '@sveltejs/adapter-static';
import { vitePreprocess } from '@sveltejs/kit/vite';

/** @type {import('@sveltejs/kit').Config} */
const config = {
	// Consult https://kit.svelte.dev/docs/integrations#preprocessors
	// for more information about preprocessors
	preprocess: vitePreprocess(),

	kit: {
		// adapter-static: https://kit.svelte.dev/docs/single-page-apps
		adapter: adapter({
            fallback: 'index.html' // may differ from host to host
        }),
	}
};

export default config;
