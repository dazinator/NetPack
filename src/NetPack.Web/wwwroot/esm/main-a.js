import usedByA from './used-by-a';
import usedByBoth from './used-by-both';

import('./dynamically-imported/apply-color-and-message').then(({ default: apply }) => {
	apply('#a [data-used-by="a"]', usedByA);
	apply('#a [data-used-by="both"]', usedByBoth);    
});