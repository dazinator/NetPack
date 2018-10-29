import usedByB from './used-by-b';
import usedByBoth from './used-by-both';

import('./dynamically-imported/apply-color-and-message').then(({ default: apply }) => {
	apply('#b [data-used-by="b"]', usedByB);
	apply('#b [data-used-by="both"]', usedByBoth);
}); 