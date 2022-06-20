const fs = require('fs')

const file = process.argv[2]
const json = JSON.parse(fs.readFileSync(file, 'utf8'))

function visit(source, deps) {
    Object.getOwnPropertyNames(deps).forEach( key => {
        console.log(`  "${source}" -> "${key}"`)

        if (deps[key].dependencies) {
            visit(key, deps[key].dependencies)
        }
    })
}


console.log('digraph {')

visit('PROJECT', json.dependencies)

console.log('}')


